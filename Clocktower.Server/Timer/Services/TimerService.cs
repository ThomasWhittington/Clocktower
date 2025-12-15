using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Timer.Services;

public class TimerService(ITimerCoordinator timerCoordinator, IGameStateStore gameStateStore) : ITimerService
{
    public TimerState GetTimer(string gameId) => timerCoordinator.Get(gameId);

    public async Task<Result<TimerState>> StartOrEditTimerAsync(string gameId, int durationSeconds, string? label = null, CancellationToken ct = default)
    {
        if (!gameStateStore.GameExists(gameId)) return Result.Fail<TimerState>(Errors.GameNotFound(gameId));
        var duration = TimeSpan.FromSeconds(durationSeconds);
        var state = await timerCoordinator.StartOrEditAsync(gameId, duration, label, ct);
        return Result.Ok(state);
    }

    public async Task<Result<TimerState>> CancelTimer(string gameId, CancellationToken ct = default)
    {
        if (!gameStateStore.GameExists(gameId)) return Result.Fail<TimerState>(Errors.GameNotFound(gameId));
        var state = await timerCoordinator.CancelAsync(gameId, ct);
        return Result.Ok(state);
    }
}