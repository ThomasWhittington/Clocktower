using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Timer.Services;

namespace Clocktower.ServerTests.Timer.Services;

[TestClass]
public class TimerServiceTests
{
    private Mock<ITimerCoordinator> _mockTimerCoordinator = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private ITimerService _sut = null!;
    private CancellationToken _ct;
    private const string GameId = "game-id";

    [TestInitialize]
    public void Setup()
    {
        _ct = CancellationToken.None;
        _mockTimerCoordinator = StrictMockFactory.Create<ITimerCoordinator>();
        _mockGameStateStore = StrictMockFactory.Create<IGameStateStore>();
        _sut = new TimerService(_mockTimerCoordinator.Object, _mockGameStateStore.Object);
    }

    private void Setup_GameStore(bool gameExists)
    {
        _mockGameStateStore.Setup(o => o.GameExists(GameId)).Returns(gameExists);
    }

    #region GetTimer

    [TestMethod]
    public void GetTimer_CallsCoordinator()
    {
        var timer = CreateTimer(GameId);
        _mockTimerCoordinator.Setup(o => o.Get(GameId)).Returns(timer);

        var result = _sut.GetTimer(GameId);

        result.Should().Be(timer);
        _mockTimerCoordinator.Verify(o => o.Get(GameId), Times.Once);
    }

    #endregion

    #region CancelTimer

    [TestMethod]
    public async Task CancelTimer_ReturnsError_WhenGameIdDoesNotExist()
    {
        Setup_GameStore(false);

        var result = await _sut.CancelTimer(GameId, _ct);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
        _mockTimerCoordinator.Verify(o => o.CancelAsync(GameId, _ct), Times.Never);
    }

    [TestMethod]
    public async Task CancelTimer_ReturnsOk_WhenGameIdExists()
    {
        var timer = CreateTimer(GameId);
        Setup_GameStore(true);
        _mockTimerCoordinator.Setup(o => o.CancelAsync(GameId, _ct)).ReturnsAsync(timer);

        var result = await _sut.CancelTimer(GameId, _ct);

        result.ShouldSucceedWith(timer);
        _mockTimerCoordinator.Verify(o => o.CancelAsync(GameId, _ct), Times.Once);
    }

    #endregion

    #region StartOrEditTimerAsync

    [TestMethod]
    public async Task StartOrEditTimerAsync_ReturnsError_WhenGameIdDoesNotExist()
    {
        const int duration = 30;
        const string label = "this-label";
        Setup_GameStore(false);

        var result = await _sut.StartOrEditTimerAsync(GameId, duration, label, _ct);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
        _mockTimerCoordinator.Verify(o => o.CancelAsync(GameId, _ct), Times.Never);
    }

    [TestMethod]
    public async Task StartOrEditTimerAsync_ReturnsOk_WhenGameIdExists()
    {
        const int duration = 30;
        const string label = "this-label";
        var timeSpan = TimeSpan.FromSeconds(duration);
        var timer = CreateTimer(GameId);
        Setup_GameStore(true);
        _mockTimerCoordinator.Setup(o => o.StartOrEditAsync(GameId, timeSpan, label, _ct)).ReturnsAsync(timer);

        var result = await _sut.StartOrEditTimerAsync(GameId, duration, label, _ct);

        result.ShouldSucceedWith(timer);
        _mockTimerCoordinator.Verify(o => o.StartOrEditAsync(GameId, timeSpan, label, _ct), Times.Once);
    }

    #endregion

    private static TimerState CreateTimer(string gameId)
    {
        return new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.None,
            ServerNowUtc = default,
            EndUtc = null,
            Label = null
        };
    }
}