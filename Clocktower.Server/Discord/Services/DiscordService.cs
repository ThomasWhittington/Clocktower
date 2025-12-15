using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Discord.Services;

public class DiscordService(IDiscordBot bot) : IDiscordService
{
    public (bool success, string guildName, string message) CheckGuildId(string guildId)
    {
        try
        {
            var guild = bot.GetGuild(guildId);
            if (guild is not null)
            {
                return (true, guild.Name, "Bot has access to guild");
            }

            return (false, string.Empty, "Bot does not have access to guild");
        }
        catch (Exception)
        {
            return (false, string.Empty, $"Bot does not have access to guild: {guildId}");
        }
    }

    public (bool success, List<MiniGuild> guilds, string message) GetGuildsWithUser(string userId)
    {
        try
        {
            List<MiniGuild> miniGuilds = [];
            var guilds = bot.GetGuilds().ToArray();
            if (!guilds.Any()) return (true, [], "Bot is not in any guilds");

            foreach (var discordGuild in guilds)
            {
                var member = discordGuild.Users.FirstOrDefault(o => o.Id == userId);
                if (member == null) continue;
                if (member.IsAdministrator())
                {
                    miniGuilds.Add(new MiniGuild(discordGuild.Id, discordGuild.Name));
                }
            }

            return (true, miniGuilds, $"Received {miniGuilds.Count} guilds where user has admin");
        }
        catch (Exception)
        {
            return (false, [], "Failed to gather guilds with user");
        }
    }

    public async Task<(bool success, string message)> SendMessage(string userId, string message)
    {
        try
        {
            var user = await bot.GetUserAsync(userId);
            if (user is null) return (false, $"Couldn't find user: {userId}");
            var dmChannel = await user.CreateDmChannelAsync();
            if (dmChannel is null) return (false, $"Failed to create dm channel for user: {userId}");
            await dmChannel.SendMessageAsync(message);
            return (true, "Sent message to user");
        }
        catch (Exception)
        {
            return (false, "Failed to send message to user");
        }
    }
}