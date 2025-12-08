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
                        occupantsList.Add(user.AsGameUser());
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
    
    public TownOccupants? UpdateUserStatus(ulong guildId, ulong userId, bool isPresent, VoiceState discordVoiceState)
    {
        var thisTownOccupancy = townOccupancyStore.Get(guildId);
        if (thisTownOccupancy is null) return null;

        var userIdString = userId.ToString();
        var newChannelCategories = thisTownOccupancy.ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                    channel with
                    {
                        Occupants = channel.Occupants.Select(occupant =>
                            occupant.Id == userIdString
                                ? occupant with { IsPresent = isPresent, VoiceState = discordVoiceState }
                                : occupant
                        ).ToList()
                    }
                ).ToList()
            }
        ).ToList();

        var newTownOccupants = new TownOccupants(newChannelCategories);
        townOccupancyStore.Set(guildId, newTownOccupants, force: true);
        return newTownOccupants;
    }
}