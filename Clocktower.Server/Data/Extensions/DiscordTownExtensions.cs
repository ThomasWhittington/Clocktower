namespace Clocktower.Server.Data.Extensions;

public static class DiscordTownExtensions
{
    public static DiscordTownDto ToDiscordTownDto(this DiscordTown discordTown, string gameId, IEnumerable<GameUser>? gameUsers = null)
    {
        gameUsers ??= [];
        var gameUserList = gameUsers.ToList();
        var gameUserLookup = gameUserList.ToDictionary(u => u.Id);
        var townUserLookup = discordTown.TownUsers.ToDictionary(u => u.Id);
        var gUsers = gameUserList.Select(o => UserDto.FromGameUser(o, townUserLookup.GetValueOrDefault(o.Id)));
        //TODO use TownDto. Probably need to move this out of an extension into a new service for dto mapping
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