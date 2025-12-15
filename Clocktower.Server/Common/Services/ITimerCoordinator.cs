namespace Clocktower.Server.Common.Services;

public interface ITimerCoordinator
{
    TimerState Get(string gameId);
    Task<TimerState> StartOrEditAsync(string gameId, TimeSpan duration, string? label = null, CancellationToken ct = default);
    Task<TimerState> CancelAsync(string gameId, CancellationToken ct = default);
}