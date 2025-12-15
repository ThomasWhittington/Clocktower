using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface IDiscordBot : IHostedService
{
    IDiscordGuild? GetGuild(string guildId);
    IEnumerable<IDiscordGuild> GetGuilds();
    Task<IDiscordUser?> GetUserAsync(string userId);
    IDiscordUser? GetUser(string userId);
}