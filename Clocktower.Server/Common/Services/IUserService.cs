namespace Clocktower.Server.Common.Services;

public interface IUserService
{
    bool UpdateDiscordPresence(string userId, string guildId, bool isPresent, VoiceState voiceState);
    string? GetUserName(string userId);

    IEnumerable<TownUser> GetTownUsersForGameUsers(IEnumerable<GameUser> users, string guildId, Func<TownUser, bool>? filter = null);
}