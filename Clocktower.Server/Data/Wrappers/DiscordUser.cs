using System.Diagnostics.CodeAnalysis;
using Discord;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordUser(IUser user) : IDiscordUser
{
    public ulong Id => user.Id;
    public string GlobalName => user.GlobalName;
    public string DisplayAvatarUrl => user.GetDisplayAvatarUrl();

    public IDiscordGuildUser? GetGuildUser()
    {
        return user is SocketGuildUser guildUser ? new DiscordGuildUser(guildUser) : null;
    }

    public async Task<IDiscordDmChannel?> CreateDmChannelAsync()
    {
        var dmChannel = await user.CreateDMChannelAsync();
        return dmChannel != null ? new DiscordDmChannel(dmChannel) : null;
    }

    public GameUser AsGameUser() => new(Id.ToString());
}