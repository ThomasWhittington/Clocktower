using DSharpPlus;

namespace Clocktower.Server.Discord.Services;

[UsedImplicitly]
public class DiscordService(DiscordBotService bot)
{
    public async Task<(bool success, bool valid, string guildName, string message)> CheckGuildId(ulong guildId)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);
            if (guild != null)
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

    public async Task<(bool success, List<MiniGuild> guilds, string message)> GetGuildsWithUser(ulong userId)
    {
        try
        {
            List<MiniGuild> miniGuilds = [];
            var guilds = bot.Client.Guilds;
            if (guilds == null || guilds.Count == 0) return (true, [], "Bot is not in any guilds");

            foreach (var discordGuild in guilds)
            {
                var member = await discordGuild.Value.GetMemberAsync(userId);
                if (member == null) continue;
                if (member.Permissions.HasPermission(Permissions.Administrator))
                {
                    miniGuilds.Add(new MiniGuild(discordGuild.Key.ToString(), discordGuild.Value.Name));
                }
            }

            return (true, miniGuilds, $"Recieved {miniGuilds.Count} guilds where user has admin");
        }
        catch (Exception)
        {
            return (false, [], "Failed to gather guilds with user");
        }
    }

    public async Task<(bool success, string message)> SendMessage(ulong userId, string requestMessage)
    {
        throw new NotImplementedException();
    }

    public record MiniGuild(string Id, string Name);
}