using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace Clocktower.Server.Data.Wrappers;

[ExcludeFromCodeCoverage(Justification = "Discord.NET wrapper")]
public class DiscordVoiceState(SocketVoiceState voiceState) : IDiscordVoiceState
{
    public IDiscordVoiceChannel? VoiceChannel => voiceState.VoiceChannel != null ? new DiscordVoiceChannel(voiceState.VoiceChannel) : null;
    public ulong? GuildId => VoiceChannel?.Guild.Id;
    public bool IsMuted => voiceState.IsMuted;
    public bool IsDeafened => voiceState.IsDeafened;
    public bool IsSelfMuted => voiceState.IsSelfMuted;
    public bool IsSelfDeafened => voiceState.IsSelfDeafened;
}