namespace Clocktower.Server.Common.Services;

public class UserService(ITownOccupancyStore townStore, IGameStateStore gameStore, ITownOccupantManager townOccupantManager)
    : IUserService
{
    public IEnumerable<TownUser> GetTownUsersForGameUsers(
        IEnumerable<GameUser> users,
        string guildId,
        Func<TownUser, bool>? filter = null)
    {
        var townOccupants = townStore.Get(guildId);
        if (townOccupants == null) yield break;

        var userIds = users.Select(p => p.Id).ToHashSet();

        foreach (
            var townUser in from category
                in townOccupants.ChannelCategories
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
        townOccupantManager.UpdateUserStatus(ulong.Parse(guildId), userId, isPresent: isPresent, discordVoiceState: voiceState);

    public bool UpdateGameUser(string gameId, string userId, UserType? userType = null, bool? isPlaying = null) =>
        gameStore.UpdateUser(gameId, ulong.Parse(userId), userType: userType, isPlaying: isPlaying);

    public string? GetUserName(string userId) => townOccupantManager.GetTownUser(userId)?.Name;
}