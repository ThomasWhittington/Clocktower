using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public class TownOccupantManager(ITownOccupancyStore townOccupancyStore) : ITownOccupantManager
{
    public TownOccupants MoveUser(TownOccupants current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel)
    {
        var currentChannel = FindUserChannel(current, user.Id.ToString());
        if (currentChannel is not null && newChannel is not null &&
            currentChannel.Channel.Id == newChannel.Id.ToString())
            return current;

        var newChannelCategories = current.ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                {
                    var occupantsList = channel.Occupants
                        .Where(o => o.Id != user.Id.ToString())
                        .ToList();

                    if (newChannel?.Id.ToString() == channel.Channel.Id)
                    {
                        occupantsList.Add(user.AsTownUser());
                    }

                    return channel with { Occupants = occupantsList };
                }).ToList()
            }
        ).ToList();
        var newTownOccupants = new TownOccupants(newChannelCategories);
        townOccupancyStore.Set(user.GuildId, newTownOccupants, force: true);
        return newTownOccupants;
    }


    public ChannelOccupants? FindUserChannel(TownOccupants occupants, string userId)
    {
        return occupants.ChannelCategories
            .SelectMany(category => category.Channels)
            .FirstOrDefault(channel => channel.Occupants.Any(occupant => occupant.Id == userId));
    }

    public bool UpdateUserStatus(ulong guildId, string userId, bool isPresent, VoiceState discordVoiceState)
    {
        var thisTownOccupancy = townOccupancyStore.Get(guildId);
        if (thisTownOccupancy is null) return false;

        var newChannelCategories = thisTownOccupancy.ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                    channel with
                    {
                        Occupants = channel.Occupants.Select(occupant =>
                            occupant.Id == userId
                                ? occupant with { IsPresent = isPresent, VoiceState = discordVoiceState }
                                : occupant
                        ).ToList()
                    }
                ).ToList()
            }
        ).ToList();

        var newTownOccupants = new TownOccupants(newChannelCategories);
        townOccupancyStore.Set(guildId, newTownOccupants, force: true);
        return true;
    }

    public TownOccupants? GetTownOccupancy(ulong guildId) => townOccupancyStore.Get(guildId);

    public TownUser? GetTownUser(string userId)
    {
        var townOccupancy = townOccupancyStore.GetTownByUser(userId);
        if (townOccupancy is null) return null;
        
        foreach (var category in townOccupancy.ChannelCategories)
        {
            foreach (var channel in category.Channels)
            {
                var user = channel.Occupants.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    return user;
                }
            }
        }
        return null;
    }
}