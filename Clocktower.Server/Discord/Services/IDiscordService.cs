namespace Clocktower.Server.Discord.Services;

public interface IDiscordService
{
    (bool success, string guildName, string message) CheckGuildId(string guildId);
    (bool success, List<MiniGuild> guilds, string message) GetGuildsWithUser(string userId);
    Task<(bool success, string message)> SendMessage(string userId, string message);
}