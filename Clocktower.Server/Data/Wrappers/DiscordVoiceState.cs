using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordVoiceState(SocketVoiceState voiceState) : IDiscordVoiceState
{
    public IDiscordVoiceChannel? VoiceChannel => voiceState.VoiceChannel != null ? new DiscordVoiceChannel(voiceState.VoiceChannel) : null;
}