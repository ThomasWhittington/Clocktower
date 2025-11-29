namespace Clocktower.Server.Discord.Town.Services;

public interface IDiscordTownService
{
    Task<(bool success, string message)> CreateTown(ulong guildId);
    Task<(bool success, string message)> DeleteTown(ulong guildId);
    Task<(bool success, string message)> MoveUser(ulong guildId, ulong userId, ulong channelId);
    (bool success, bool exists, string message) GetTownStatus(ulong guildId);
    Task<(bool success, string message)> ToggleStoryTeller(string gameId, ulong userId);
    Task<(InviteUserOutcome outcome, string message)> InviteUser(string gameId, ulong userId);
    Task<(bool success, TownOccupants? townOccupants, string message)> GetTownOccupancy(ulong guildId);
    JoinData? GetJoinData(string key);
    Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime);
    Task PingUser(string userId);
}