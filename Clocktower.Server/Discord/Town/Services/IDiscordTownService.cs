namespace Clocktower.Server.Discord.Town.Services;

public interface IDiscordTownService
{
    Task<(bool success, string message)> CreateTown(ulong guildId);
    Task<(bool success, string message)> DeleteTown(ulong guildId);
    Task<(bool success, string message)> MoveUser(ulong guildId, ulong userId, ulong channelId);
    Task<(bool success, string message)> RebuildTown(ulong guildId);
    (bool success, bool exists, string message) TownExists(ulong guildId);
    Task<(bool success, string message)> ToggleStoryTeller(ulong guildId, ulong userId);
    Task<(InviteUserOutcome outcome, string message)> InviteUser(string gameId, ulong userId);
    Task<(bool success, TownOccupants? townOccupants, string message)> GetTownOccupancy(ulong guildId);
    JoinData? GetJoinData(string key);
    Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime);
}