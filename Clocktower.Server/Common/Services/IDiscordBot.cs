using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public interface IDiscordBot : IHostedService
{
    IDiscordGuild? GetGuild(ulong guildId);
    IEnumerable<IDiscordGuild> GetGuilds();

    Task<IDiscordUser?> GetUserAsync(ulong userId);
    IDiscordUser? GetUser(ulong userId);
}