using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordVoiceChannel(SocketVoiceChannel channel) : IDiscordVoiceChannel
{
    public string Id => channel.Id.ToString();
    public string Name => channel.Name;
    public IEnumerable<IDiscordGuildUser> ConnectedUsers => channel.ConnectedUsers.Select(user => new DiscordGuildUser(user));
    public string? CategoryId => channel.CategoryId?.ToString();
    public int Position => channel.Position;
    public IDiscordGuild Guild => new DiscordGuild(channel.Guild);
}