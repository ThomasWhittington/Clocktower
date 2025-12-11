namespace Clocktower.Server.Data.Extensions;

public static class DiscordTownExtensions
{
    public static DiscordTownDto ToDiscordTownDto(this DiscordTown discordTown, string gameId, IEnumerable<GameUser>? gameUsers = null)
    {
        var gameUserLookup = gameUsers?.ToDictionary(u => u.Id) ?? new Dictionary<string, GameUser>();

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

        return new DiscordTownDto(gameId, categoriesDto);
    }
}