using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordVoiceChannel(SocketVoiceChannel channel) : IDiscordVoiceChannel
{
    public ulong Id => channel.Id;
    public string Name => channel.Name;
    public IEnumerable<IDiscordGuildUser> ConnectedUsers => channel.ConnectedUsers.Select(user => new DiscordGuildUser(user));
    public ulong? CategoryId => channel.CategoryId;
    public int Position => channel.Position;
    public IDiscordGuild Guild => new DiscordGuild(channel.Guild);
} 