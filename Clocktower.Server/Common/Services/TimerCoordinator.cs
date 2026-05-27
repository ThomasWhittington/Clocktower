using System.Collections.Concurrent;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Common.Services;

public class TimerCoordinator(ILogger<TimerCoordinator> logger, IGameBroadcastService gameBroadcastService) : ITimerCoordinator
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

        await gameBroadcastService.BroadcastTimerUpdate(gameId, state);
        await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.Stop);

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

        await gameBroadcastService.BroadcastTimerUpdate(gameId, state);
        await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.Stop);
        return state;
    }

    private async Task FinishLaterAsync(string gameId, DateTimeOffset endUtc, CancellationToken ct)
    {
        try
        {
            var delay = endUtc - DateTimeOffset.UtcNow;
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            var tenSecondsBeforeEnd = delay - TimeSpan.FromSeconds(10);
            if (tenSecondsBeforeEnd > TimeSpan.Zero)
            {
                await Task.Delay(tenSecondsBeforeEnd, ct);

                if (!_timers.TryGetValue(gameId, out var inst))
                    return;

                if (inst.State.Status != TimerStatus.Running || inst.State.EndUtc != endUtc)
                    return;

                await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.Timer10Seconds);

                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
            else
            {
                await Task.Delay(delay, ct);
            }

            if (!_timers.TryGetValue(gameId, out var inst2))
                return;

            if (inst2.State.Status != TimerStatus.Running || inst2.State.EndUtc != endUtc)
                return;

            var finished = inst2.State with
            {
                Status = TimerStatus.Finished,
                ServerNowUtc = DateTimeOffset.UtcNow
            };

            inst2.State = finished;
            await gameBroadcastService.BroadcastTimerUpdate(gameId, finished);
            await gameBroadcastService.BroadcastPlayAudio(gameId, AudioEvent.TimerUp);
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