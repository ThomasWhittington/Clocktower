using System.Collections.Concurrent;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Common.Services;

public class TimerCoordinator(ILogger<TimerCoordinator> logger, INotificationService notifications) : ITimerCoordinator
{
    internal sealed class TimerInstance
    {
        public required TimerState State { get; set; }
        public required CancellationTokenSource Cts { get; init; }
    }

    private readonly ConcurrentDictionary<string, TimerInstance> _timers = new();

    public TimerState Get(string gameId)
    {
        if (_timers.TryGetValue(gameId, out var inst))
            return inst.State with { ServerNowUtc = DateTimeOffset.UtcNow };

        return new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.None,
            ServerNowUtc = DateTimeOffset.UtcNow,
            EndUtc = null,
            Label = null
        };
    }

    public async Task<TimerState> StartOrEditAsync(string gameId, TimeSpan duration, string? label = null, CancellationToken ct = default)
    {
        if (_timers.TryGetValue(gameId, out var existing))
        {
            await CancelAndDisposeAsync(existing.Cts);
        }

        var now = DateTimeOffset.UtcNow;
        var end = now.Add(duration);

        var cts = new CancellationTokenSource();
        var state = new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.Running,
            ServerNowUtc = now,
            EndUtc = end,
            Label = label
        };

        _timers[gameId] = new TimerInstance { State = state, Cts = cts };

        await notifications.BroadcastTimerUpdate(gameId, state);

        _ = FinishLaterAsync(gameId, end, cts.Token);

        return state;
    }

    public async Task<TimerState> CancelAsync(string gameId, CancellationToken ct = default)
    {
        if (_timers.TryGetValue(gameId, out var existing))
        {
            await CancelAndDisposeAsync(existing.Cts);
        }

        var state = new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.Cancelled,
            ServerNowUtc = DateTimeOffset.UtcNow,
            EndUtc = null,
            Label = null
        };

        _timers[gameId] = new TimerInstance { State = state, Cts = new CancellationTokenSource() };

        await notifications.BroadcastTimerUpdate(gameId, state);
        return state;
    }

    private async Task FinishLaterAsync(string gameId, DateTimeOffset endUtc, CancellationToken ct)
    {
        try
        {
            var delay = endUtc - DateTimeOffset.UtcNow;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            await Task.Delay(delay, ct);

            if (!_timers.TryGetValue(gameId, out var inst))
                return;
            
            if (inst.State.Status != TimerStatus.Running || inst.State.EndUtc != endUtc)
                return;

            var finished = inst.State with
            {
                Status = TimerStatus.Finished,
                ServerNowUtc = DateTimeOffset.UtcNow
            };

            inst.State = finished;
            await notifications.BroadcastTimerUpdate(gameId, finished);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finishing timer for game {GameId}", gameId);
        }
    }

    private static async Task CancelAndDisposeAsync(CancellationTokenSource cts)
    {
        try
        {
            await cts.CancelAsync();
        }
        finally
        {
            cts.Dispose();
        }
    }
}