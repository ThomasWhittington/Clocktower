namespace Clocktower.Server.Discord.Services;

public class DiscordService(NewDiscordBotService bot) : IDiscordService
{
    public (bool success, bool valid, string guildName, string message) CheckGuildId(ulong guildId)
    {
        try
        {
            var guild = bot.Client.GetGuild(guildId);
            if (guild is not null)
            {
                return (true, true, guild.Name, "Bot has access to guild");
            }

            return (false, false, string.Empty, "Bot does not have access to guild");
        }
        catch (Exception)
        {
            return (false, false, string.Empty, $"Bot does not have access to guild: {guildId}");
        }
    }

    public (bool success, List<MiniGuild> guilds, string message) GetGuildsWithUser(ulong userId)
    {
        try
        {
            List<MiniGuild> miniGuilds = [];
            var guilds = bot.Client.Guilds;
            if (guilds == null || guilds.Count == 0) return (true, [], "Bot is not in any guilds");

            foreach (var discordGuild in guilds)
            {
                var member = discordGuild.Users.FirstOrDefault(o => o.Id == userId);
                if (member == null) continue;
                if (member.GuildPermissions.Administrator)
                {
                    miniGuilds.Add(new MiniGuild(discordGuild.Id.ToString(), discordGuild.Name));
                }
            }

            return (true, miniGuilds, $"Received {miniGuilds.Count} guilds where user has admin");
        }
        catch (Exception)
        {
            return (false, [], "Failed to gather guilds with user");
        }
    }

    public async Task<(bool success, string message)> SendMessage(ulong userId, string message)
    {
        throw new NotImplementedException();
    }
}