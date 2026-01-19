namespace Clocktower.Server.Data;

public record VoiceState(bool IsPresent, bool IsServerMuted, bool IsServerDeafened, bool IsSelfMuted, bool IsSelfDeafened);