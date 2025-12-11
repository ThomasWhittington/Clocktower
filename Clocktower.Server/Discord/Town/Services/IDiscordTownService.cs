namespace Clocktower.Server.Discord.Town.Services;

public interface IDiscordTownService
{
    Task<(bool success, string message)> CreateTown(ulong guildId);
    Task<(bool success, string message)> DeleteTown(ulong guildId);
    Task<(bool success, string message)> MoveUser(ulong guildId, ulong userId, ulong channelId);
    (bool success, bool exists, string message) GetTownStatus(ulong guildId);
    Task<(bool success, string message)> ToggleStoryTeller(string gameId, ulong userId);
    Task<(InviteUserOutcome outcome, string message)> InviteUser(string gameId, ulong userId);
    Task<(bool success, DiscordTown? discordTown, string message)> GetDiscordTown(ulong guildId);
    Task<(bool success, DiscordTownDto? discordTown, string message)> GetDiscordTownDto(string gameId);
    JoinData? GetJoinData(string key);
    Task PingUser(string userId);
}