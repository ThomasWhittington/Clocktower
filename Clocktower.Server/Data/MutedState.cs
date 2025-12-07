namespace Clocktower.Server.Data;

public record MutedState(bool IsServerMuted, bool IsServerDeafened, bool IsSelfMuted, bool IsSelfDeafened);