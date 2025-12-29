using Clocktower.Server.Data.Dto;

namespace Clocktower.Server.Data;

public class SessionSyncState
{
    public required GameTime GameTime { get; init; }
    public required string Jwt { get; init; }
    public required DiscordTownDto? DiscordTown { get; init; }
    public required TimerState Timer { get; init; }
}