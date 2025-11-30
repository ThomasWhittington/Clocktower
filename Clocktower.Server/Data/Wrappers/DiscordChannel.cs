using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordChannel(SocketGuildChannel channel) : IDiscordChannel
{
    public string Name => channel.Name;

    public async Task DeleteAsync()
    {
        await channel.DeleteAsync();
    }
}