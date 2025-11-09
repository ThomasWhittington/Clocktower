namespace Clocktower.Server.Discord.Services;

public interface IDiscordService
{
    (bool success, bool valid, string guildName, string message) CheckGuildId(ulong guildId);
    (bool success, List<MiniGuild> guilds, string message) GetGuildsWithUser(ulong userId);
    Task<(bool success, string message)> SendMessage(ulong userId, string message);
}