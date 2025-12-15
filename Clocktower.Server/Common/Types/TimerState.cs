namespace Clocktower.Server.Common.Types;

public sealed record TimerState
{
    public required string GameId { get; init; }
    public required TimerStatus Status { get; init;}
    public required DateTimeOffset ServerNowUtc { get; set; }
    public required DateTimeOffset? EndUtc{ get; init; }
    public string? Label { get; set; }
}