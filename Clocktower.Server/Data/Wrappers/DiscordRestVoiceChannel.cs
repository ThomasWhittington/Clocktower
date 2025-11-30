using System.Diagnostics.CodeAnalysis;
using Discord.Rest;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordRestVoiceChannel(RestVoiceChannel channel) : IDiscordRestVoiceChannel
{
}