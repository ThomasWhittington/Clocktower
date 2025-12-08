namespace Clocktower.Server.Data;

public record VoiceState(bool IsServerMuted, bool IsServerDeafened, bool IsSelfMuted, bool IsSelfDeafened);