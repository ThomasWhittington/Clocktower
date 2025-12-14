using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public class DiscordTownManager(IDiscordTownStore discordTownStore) : IDiscordTownManager
{
    public DiscordTown MoveUser(DiscordTown current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel)
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
        var discordTown = new DiscordTown(newChannelCategories);
        discordTownStore.Set(user.GuildId, discordTown, force: true);
        return discordTown;
    }


    public ChannelOccupants? FindUserChannel(DiscordTown occupants, string userId)
    {
        return occupants.ChannelCategories
            .SelectMany(category => category.Channels)
            .FirstOrDefault(channel => channel.Occupants.Any(occupant => occupant.Id == userId));
    }

    public bool UpdateUserStatus(ulong guildId, string userId, bool isPresent, VoiceState discordVoiceState)
    {
        var discordTown = discordTownStore.Get(guildId);
        if (discordTown is null) return false;

        var newChannelCategories = discordTown.ChannelCategories.Select(category =>
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

        var newDiscordTown = new DiscordTown(newChannelCategories);
        discordTownStore.Set(guildId, newDiscordTown, force: true);
        return true;
    }

    public DiscordTown? GetDiscordTown(ulong guildId) => discordTownStore.Get(guildId);

    public ulong? GetVoiceChannelIdByName(ulong guildId, string voiceChannelName)
    {
        var town = GetDiscordTown(guildId);

        var channelIdStr = town?.ChannelCategories
            .SelectMany(category => category.Channels)
            .FirstOrDefault(ch => ch.Channel.Name == voiceChannelName)
            ?.Channel.Id;

        return ulong.TryParse(channelIdStr, out var result) ? result : null;
    }


    public TownUser? GetTownUser(string userId)
    {
        var discordTown = discordTownStore.GetTownByUser(userId);
        if (discordTown is null) return null;

        foreach (var category in discordTown.ChannelCategories)
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

    public IEnumerable<MiniChannel> GetNightChannels(ulong guildId, string categoryName)
    {
        var town = GetDiscordTown(guildId);
        if (town is null) return [];
        var nightCategory = town.ChannelCategories.FirstOrDefault(o => o.Name == categoryName);
        if (nightCategory is null) return [];
        var cottages = nightCategory.Channels.Select(channel => channel.Channel).ToList();
        return cottages;
    }
}