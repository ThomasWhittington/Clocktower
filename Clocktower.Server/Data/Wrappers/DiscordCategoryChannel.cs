using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordCategoryChannel(SocketCategoryChannel channel) : IDiscordCategoryChannel
{
    public ulong Id => channel.Id;
    public string Name => channel.Name;
    public IEnumerable<IDiscordChannel> Channels => channel.Channels.Select(c => new DiscordChannel(c));

    public async Task DeleteAsync()
    {
        await channel.DeleteAsync();
    }
}