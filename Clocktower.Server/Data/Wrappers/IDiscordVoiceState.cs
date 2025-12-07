namespace Clocktower.Server.Data.Wrappers;

public interface IDiscordVoiceState
{
    IDiscordVoiceChannel? VoiceChannel { get; }
    ulong? GuildId { get; }
    public bool IsMuted { get; }
    public bool IsDeafened { get; }
    public bool IsSelfMuted { get; }
    public bool IsSelfDeafened { get; }
}