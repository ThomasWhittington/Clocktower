namespace Clocktower.Server.Common.Services;

public class UserService(IDiscordTownStore discordTownStore, IGameStateStore gameStore, IDiscordTownManager discordTownManager)
    : IUserService
{
    public IEnumerable<TownUser> GetTownUsersForGameUsers(
        IEnumerable<GameUser> users,
        string guildId,
        Func<TownUser, bool>? filter = null)
    {
        var discordTown = discordTownStore.Get(guildId);
        if (discordTown == null) yield break;

        var userIds = users.Select(p => p.Id).ToHashSet();

        foreach (
            var townUser in from category
                in discordTown.ChannelCategories
            from channel in category.Channels
            from townUser in channel.Occupants
            where userIds.Contains(townUser.Id) &&
                  (filter == null || filter(townUser))
            select townUser)
        {
            yield return townUser;
        }
    }

    public bool UpdateDiscordPresence(string userId, string guildId, bool isPresent, VoiceState voiceState) =>
        discordTownManager.UpdateUserStatus(guildId, userId, isPresent: isPresent, discordVoiceState: voiceState);

    public bool UpdateGameUser(string gameId, string userId, UserType? userType = null, bool? isPlaying = null) =>
        gameStore.UpdateUser(gameId, userId, userType: userType, isPlaying: isPlaying);

    public string? GetUserName(string userId) => discordTownManager.GetTownUser(userId)?.Name;
}