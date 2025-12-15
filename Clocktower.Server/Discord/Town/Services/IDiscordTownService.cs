namespace Clocktower.Server.Discord.Town.Services;

public interface IDiscordTownService
{
    Task<(bool success, string message)> CreateTown(string guildId);
    Task<(bool success, string message)> DeleteTown(string guildId);
    Task<(bool success, string message)> MoveUser(string guildId, string userId, string channelId);
    (bool success, bool exists, string message) GetTownStatus(string guildId);
    Task<(bool success, string message)> ToggleStoryTeller(string gameId, string userId);
    Task<(InviteUserOutcome outcome, string message)> InviteUser(string gameId, string userId);
    Task<(bool success, DiscordTown? discordTown, string message)> GetDiscordTown(string guildId);
    Task<(bool success, DiscordTownDto? discordTown, string message)> GetDiscordTownDto(string gameId);
    JoinData? GetJoinData(string key);
    Task PingUser(string userId);
}