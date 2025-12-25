using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord;

namespace Clocktower.Server.Common.Services;

public class DiscordTownManager(IDiscordTownStore discordTownStore, IUserIdentityStore userIdentityStore, IDiscordConstantsService discordConstantsService) : IDiscordTownManager
{
    public DiscordTown MoveUser(DiscordTown current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel)
    {
        var currentChannel = FindUserChannel(current, user.Id);
        if (currentChannel is not null && newChannel is not null &&
            currentChannel.Channel.Id == newChannel.Id)
            return current;

        var newChannelCategories = current.ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                {
                    var occupantsList = channel.Occupants
                        .Where(o => o.Id != user.Id)
                        .ToList();

                    if (newChannel?.Id == channel.Channel.Id)
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

    public bool UpdateUserStatus(string guildId, string userId, bool isPresent, VoiceState discordVoiceState)
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

    public DiscordTown? GetDiscordTown(string guildId) => discordTownStore.Get(guildId);

    public string? GetVoiceChannelIdByName(string guildId, string voiceChannelName)
    {
        var town = GetDiscordTown(guildId);

        var channelId = town?.ChannelCategories
            .SelectMany(category => category.Channels)
            .FirstOrDefault(ch => ch.Channel.Name == voiceChannelName)
            ?.Channel.Id;

        return channelId;
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

    public IEnumerable<MiniChannel> GetNightChannels(string guildId, string categoryName)
    {
        var town = GetDiscordTown(guildId);
        if (town is null) return [];
        var nightCategory = town.ChannelCategories.FirstOrDefault(o => o.Name == categoryName);
        if (nightCategory is null) return [];
        var cottages = nightCategory.Channels.Select(channel => channel.Channel).ToList();
        return cottages;
    }


    public DiscordTownDto RedactTownDto(DiscordTownDto discordTownDto, string userId)
    {
        var redactedCategories = discordTownDto.ChannelCategories
            .Select(category =>
            {
                if (!string.Equals(category.Name, discordConstantsService.NightCategoryName, StringComparison.OrdinalIgnoreCase))
                    return category;

                var visibleNightChannel = category.Channels.FirstOrDefault(ch => ch.Occupants.Any(o => o.Id == userId));

                var redactedNightChannels = visibleNightChannel is null
                    ? Enumerable.Empty<ChannelOccupantsDto>()
                    : new[] { visibleNightChannel };

                return category with { Channels = redactedNightChannels };
            })
            .Where(category => category.Channels.Any())
            .ToList();

        return discordTownDto with { ChannelCategories = redactedCategories };
    }

    public bool SetDiscordTown(string guildId, DiscordTown discordTown) => discordTownStore.Set(guildId, discordTown);
    public void UpdateUserIdentity(TownUser townUser) => userIdentityStore.UpdateIdentity(townUser);

    public DiscordTownDto? GetDiscordTownDto(string guildId, string gameId, IEnumerable<GameUser>? gameUsers = null)
    {
        var discordTown = GetDiscordTown(guildId);
        if (discordTown is null) return null;
        return GetDiscordTownDto(discordTown, gameId, gameUsers);
    }

    public DiscordTownDto? GetDiscordTownDto(DiscordTown? discordTown, string gameId, IEnumerable<GameUser>? gameUsers = null)
    {
        if (discordTown is null) return null;

        foreach (var user in discordTown.TownUsers) userIdentityStore.UpdateIdentity(user);

        gameUsers ??= [];
        var gameUserList = gameUsers.ToList();
        var gameUserLookup = gameUserList.ToDictionary(u => u.Id);
        var gUsers = gameUserList.Select(o =>
        {
            var townUser = userIdentityStore.GetIdentity(o.Id);
            return UserDto.FromGameUser(o, townUser);
        }).ToList();

        var categoriesDto = discordTown.ChannelCategories.Select(category =>
            new MiniCategoryDto(
                category.Id,
                category.Name,
                category.Channels.Select(channel =>
                    new ChannelOccupantsDto(
                        channel.Channel,
                        channel.Occupants.Select(townUser =>
                            UserDto.FromTownUser(townUser, gameUserLookup.GetValueOrDefault(townUser.Id))
                        ).ToList()
                    )
                ).ToList()
            )
        ).ToList();

        var town = new DiscordTownDto(gameId, categoriesDto)
        {
            GameUsers = gUsers
        };

        return town;
    }
}