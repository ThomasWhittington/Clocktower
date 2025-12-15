namespace Clocktower.Server.Timer.Services;

public interface ITimerService
{
    Task<Result<TimerState>> StartOrEditAsync(string gameId, int durationSeconds, string? label = null, CancellationToken ct = default);
    Task<Result<TimerState>> CancelTimer(string gameId, CancellationToken ct = default);
    TimerState GetTimer(string gameId);
}