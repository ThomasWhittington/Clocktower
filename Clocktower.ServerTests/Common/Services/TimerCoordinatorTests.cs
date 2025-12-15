using System.Collections.Concurrent;
using System.Reflection;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket;
using Microsoft.Extensions.Logging;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class TimerCoordinatorTests
{
    private Mock<ILogger<TimerCoordinator>> _logger = null!;
    private Mock<INotificationService> _notifications = null!;
    private TimerCoordinator _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new Mock<ILogger<TimerCoordinator>>(MockBehavior.Loose);
        _notifications = new Mock<INotificationService>(MockBehavior.Strict);

        _sut = new TimerCoordinator(_logger.Object, _notifications.Object);
    }

    [TestMethod]
    public void Get_ReturnsDefaultNoneTimer_WhenNoTimerExists()
    {
        var before = DateTimeOffset.UtcNow;
        var result = _sut.Get("game-1");
        var after = DateTimeOffset.UtcNow;

        result.GameId.Should().Be("game-1");
        result.Status.Should().Be(TimerStatus.None);
        result.EndUtc.Should().BeNull();
        result.Label.Should().BeNull();
        result.ServerNowUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [TestMethod]
    public async Task Get_ReturnsExistingTimer_WithRefreshedServerNowUtc()
    {
        _notifications
            .Setup(n => n.BroadcastTimerUpdate(It.IsAny<string>(), It.IsAny<TimerState>()))
            .Returns(Task.CompletedTask);

        var started = await _sut.StartOrEditAsync("game-1", TimeSpan.FromSeconds(10), label: "lbl");

        await Task.Delay(10);

        var got = _sut.Get("game-1");

        got.GameId.Should().Be("game-1");
        got.Status.Should().Be(TimerStatus.Running);
        got.EndUtc.Should().Be(started.EndUtc);
        got.Label.Should().Be("lbl");

        got.ServerNowUtc.Should().BeAfter(started.ServerNowUtc);
        _notifications.Verify(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task StartOrEditAsync_SetsRunningState_AndBroadcastsRunningUpdate()
    {
        var updates = new ConcurrentQueue<TimerState>();

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) => updates.Enqueue(state))
            .Returns(Task.CompletedTask);

        var before = DateTimeOffset.UtcNow;

        var result = await _sut.StartOrEditAsync("game-1", TimeSpan.FromSeconds(30), label: "hello");

        var after = DateTimeOffset.UtcNow;

        result.GameId.Should().Be("game-1");
        result.Status.Should().Be(TimerStatus.Running);
        result.Label.Should().Be("hello");
        result.EndUtc.Should().NotBeNull();
        result.EndUtc.Should().BeOnOrAfter(before.AddSeconds(30)).And.BeOnOrBefore(after.AddSeconds(30));
        result.ServerNowUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);

        await WaitUntilAsync(
            condition: () => !updates.IsEmpty,
            timeout: TimeSpan.FromSeconds(1),
            pollInterval: TimeSpan.FromMilliseconds(5));

        updates.Should().ContainSingle(s => s.Status == TimerStatus.Running && s.Label == "hello");

        _notifications.Verify(
            n => n.BroadcastTimerUpdate("game-1",
                It.Is<TimerState>(s => s.Status == TimerStatus.Running && s.Label == "hello")),
            Times.Once);
    }

    [TestMethod]
    public async Task StartOrEditAsync_WithZeroDuration_FinishesAndBroadcastsFinishedUpdate()
    {
        var runningTcs = new TaskCompletionSource<TimerState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishedTcs = new TaskCompletionSource<TimerState>(TaskCreationOptions.RunContinuationsAsynchronously);

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) =>
            {
                switch (state.Status)
                {
                    case TimerStatus.Running:
                        runningTcs.TrySetResult(state);
                        break;
                    case TimerStatus.Finished:
                        finishedTcs.TrySetResult(state);
                        break;
                }
            })
            .Returns(Task.CompletedTask);

        var startResult = await _sut.StartOrEditAsync("game-1", TimeSpan.Zero, label: "instant");

        var running = await runningTcs.Task.WithTimeout(TimeSpan.FromSeconds(1));
        var finished = await finishedTcs.Task.WithTimeout(TimeSpan.FromSeconds(1));

        running.Status.Should().Be(TimerStatus.Running);
        running.EndUtc.Should().Be(startResult.EndUtc);

        finished.Status.Should().Be(TimerStatus.Finished);
        finished.EndUtc.Should().Be(startResult.EndUtc);
        finished.ServerNowUtc.Should().BeAfter(running.ServerNowUtc);

        _notifications.Verify(
            n => n.BroadcastTimerUpdate("game-1", It.Is<TimerState>(s => s.Status == TimerStatus.Running)),
            Times.Once);

        _notifications.Verify(
            n => n.BroadcastTimerUpdate("game-1", It.Is<TimerState>(s => s.Status == TimerStatus.Finished)),
            Times.Once);
    }

    [TestMethod]
    public async Task StartOrEditAsync_WhenEdited_CancelsOldTimerFinish_AndOnlyFinishesLatest()
    {
        var updates = new ConcurrentQueue<TimerState>();

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) => updates.Enqueue(state))
            .Returns(Task.CompletedTask);

        var first = await _sut.StartOrEditAsync("game-1", TimeSpan.FromMilliseconds(60), label: "first");
        var second = await _sut.StartOrEditAsync("game-1", TimeSpan.FromMilliseconds(200), label: "second");

        await Task.Delay(120);

        updates
            .Where(s => s.Status == TimerStatus.Finished)
            .Should()
            .OnlyContain(s => s.EndUtc == second.EndUtc);

        await WaitUntilAsync(
            condition: () => updates.Any(s => s.Status == TimerStatus.Finished && s.EndUtc == second.EndUtc),
            timeout: TimeSpan.FromSeconds(2),
            pollInterval: TimeSpan.FromMilliseconds(10));

        updates.Should().Contain(s => s.Status == TimerStatus.Running && s.EndUtc == first.EndUtc && s.Label == "first");
        updates.Should().Contain(s => s.Status == TimerStatus.Running && s.EndUtc == second.EndUtc && s.Label == "second");
        updates.Should().Contain(s => s.Status == TimerStatus.Finished && s.EndUtc == second.EndUtc);
    }

    [TestMethod]
    public async Task StartOrEditAsync_WhenTimerEntryIsMissing_WhenFinishRuns_DoesNotBroadcastFinished()
    {
        var finishedTcs = new TaskCompletionSource<TimerState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var runningTcs = new TaskCompletionSource<TimerState>(TaskCreationOptions.RunContinuationsAsynchronously);

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) =>
            {
                if (state.Status == TimerStatus.Running) runningTcs.TrySetResult(state);
                if (state.Status == TimerStatus.Finished) finishedTcs.TrySetResult(state);
            })
            .Returns(Task.CompletedTask);

        await _sut.StartOrEditAsync("game-1", TimeSpan.FromMilliseconds(80), label: "will-be-removed");

        await runningTcs.Task.WithTimeout(TimeSpan.FromSeconds(1));

        RemoveTimerEntryViaReflection(_sut, "game-1");

        await AssertNotCompletedWithinAsync(finishedTcs.Task, TimeSpan.FromMilliseconds(250));

        _notifications.Verify(
            n => n.BroadcastTimerUpdate("game-1", It.Is<TimerState>(s => s.Status == TimerStatus.Finished)),
            Times.Never);
    }

    [TestMethod]
    public async Task StartOrEditAsync_WhenEditedBeforeFirstFinishes_OldFinishReturnsDueToEndMismatch_AndOnlyLatestFinishes()
    {
        var updates = new ConcurrentQueue<TimerState>();

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) => updates.Enqueue(state))
            .Returns(Task.CompletedTask);

        var first = await _sut.StartOrEditAsync("game-1", TimeSpan.FromMilliseconds(80), label: "first");
        var second = await _sut.StartOrEditAsync("game-1", TimeSpan.FromMilliseconds(250), label: "second");

        await Task.Delay(140);

        updates.Should().NotContain(s => s.Status == TimerStatus.Finished && s.EndUtc == first.EndUtc);

        await WaitUntilAsync(
            condition: () => updates.Any(s => s.Status == TimerStatus.Finished && s.EndUtc == second.EndUtc),
            timeout: TimeSpan.FromSeconds(2),
            pollInterval: TimeSpan.FromMilliseconds(10));

        updates.Should().Contain(s => s.Status == TimerStatus.Finished && s.EndUtc == second.EndUtc);
    }

    [TestMethod]
    public async Task StartOrEditAsync_WhenEndUtcIsDifferentAtFinishTime_FinishReturns_AndDoesNotBroadcastFinished()
    {
        var finishedTcs = new TaskCompletionSource<TimerState>(TaskCreationOptions.RunContinuationsAsynchronously);

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) =>
            {
                if (state.Status == TimerStatus.Finished)
                {
                    finishedTcs.TrySetResult(state);
                }
            })
            .Returns(Task.CompletedTask);

        var started = await _sut.StartOrEditAsync("game-1", TimeSpan.FromMilliseconds(80), label: "x");

        UpdateTimerStateViaReflection(
            _sut,
            "game-1",
            s => s with { EndUtc = started.EndUtc!.Value.AddMilliseconds(500) });

        await AssertNotCompletedWithinAsync(finishedTcs.Task, TimeSpan.FromMilliseconds(250));

        _notifications.Verify(
            n => n.BroadcastTimerUpdate("game-1", It.Is<TimerState>(s => s.Status == TimerStatus.Finished)),
            Times.Never);

        _sut.Get("game-1").EndUtc.Should().NotBe(started.EndUtc);
        _sut.Get("game-1").Status.Should().Be(TimerStatus.Running);
    }

    [TestMethod]
    public async Task CancelAsync_SetsCancelledState_StoresIt_AndBroadcastsCancelledUpdate()
    {
        var updates = new ConcurrentQueue<TimerState>();

        _notifications
            .Setup(n => n.BroadcastTimerUpdate("game-1", It.IsAny<TimerState>()))
            .Callback<string, TimerState>((_, state) => updates.Enqueue(state))
            .Returns(Task.CompletedTask);

        await _sut.StartOrEditAsync("game-1", TimeSpan.FromSeconds(10), label: "x");

        var cancelResult = await _sut.CancelAsync("game-1");

        cancelResult.GameId.Should().Be("game-1");
        cancelResult.Status.Should().Be(TimerStatus.Cancelled);
        cancelResult.EndUtc.Should().BeNull();
        cancelResult.Label.Should().BeNull();

        var got = _sut.Get("game-1");
        got.Status.Should().Be(TimerStatus.Cancelled);
        got.EndUtc.Should().BeNull();

        _notifications.Verify(
            n => n.BroadcastTimerUpdate("game-1", It.Is<TimerState>(s => s.Status == TimerStatus.Cancelled)),
            Times.Once);

        updates.Should().Contain(s => s.Status == TimerStatus.Cancelled);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout, TimeSpan pollInterval)
    {
        var start = DateTimeOffset.UtcNow;
        while (!condition())
        {
            if (DateTimeOffset.UtcNow - start > timeout)
                throw new AssertFailedException($"Condition was not met within {timeout}.");

            await Task.Delay(pollInterval);
        }
    }

    private static void RemoveTimerEntryViaReflection(TimerCoordinator sut, string gameId)
    {
        var timersField = typeof(TimerCoordinator)
            .GetField("_timers", BindingFlags.Instance | BindingFlags.NonPublic);

        timersField.Should().NotBeNull("TimerCoordinator should have a private _timers field.");

        var timers = timersField!.GetValue(sut);
        timers.Should().NotBeNull("_timers should be initialized.");

        var dict = (ConcurrentDictionary<string, TimerCoordinator.TimerInstance>)timers!;
        dict.TryRemove(gameId, out _);
    }

    private static async Task AssertNotCompletedWithinAsync(Task task, TimeSpan duration)
    {
        var completed = await Task.WhenAny(task, Task.Delay(duration));
        if (completed == task)
            throw new AssertFailedException($"Expected task to NOT complete within {duration}, but it completed.");
    }

    private static void UpdateTimerStateViaReflection(TimerCoordinator sut, string gameId, Func<TimerState, TimerState> mutate)
    {
        var timersField = typeof(TimerCoordinator)
            .GetField("_timers", BindingFlags.Instance | BindingFlags.NonPublic);
        timersField.Should().NotBeNull("TimerCoordinator should have a private _timers field.");

        var timersObj = timersField!.GetValue(sut);
        timersObj.Should().NotBeNull("_timers should be initialized.");

        var timersType = timersObj!.GetType();

        var tryGetValue = timersType.GetMethod("TryGetValue", [typeof(string), timersType.GenericTypeArguments[1].MakeByRefType()]);
        tryGetValue.Should().NotBeNull("ConcurrentDictionary should have TryGetValue(key, out value).");

        var args = new object?[] { gameId, null };
        var found = (bool)tryGetValue!.Invoke(timersObj, args)!;
        found.Should().BeTrue("Timer entry should exist for mutation test.");

        var inst = args[1]!;
        var instType = inst.GetType();

        var stateProp = instType.GetProperty("State", BindingFlags.Instance | BindingFlags.Public);
        stateProp.Should().NotBeNull("TimerInstance should have a State property.");

        var currentState = (TimerState)stateProp!.GetValue(inst)!;
        var newState = mutate(currentState);

        stateProp.SetValue(inst, newState);
    }
}

internal static class TaskTimeoutExtensions
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var delayTask = Task.Delay(timeout, cts.Token);

        var completed = await Task.WhenAny(task, delayTask);
        if (completed == delayTask)
            throw new AssertFailedException($"Timed out after {timeout}.");

        await cts.CancelAsync();
        return await task;
    }
}