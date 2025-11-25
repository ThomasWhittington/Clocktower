using System.Diagnostics.CodeAnalysis;
using Discord;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordDmChannel(IDMChannel dmChannel) : IDiscordDmChannel
{
    public async Task SendMessageAsync(string message)
    {
        await dmChannel.SendMessageAsync(message);
    }
}