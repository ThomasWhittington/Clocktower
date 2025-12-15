using Clocktower.Server.Timer.Endpoints;
using Clocktower.Server.Timer.Services;

namespace Clocktower.ServerTests.Timer.Endpoints;

[TestClass]
public class CancelTimerTests
{
    private Mock<ITimerService> _mockTimerService = null!;
    private CancellationToken _ct;

    [TestInitialize]
    public void Setup()
    {
        _mockTimerService = StrictMockFactory.Create<ITimerService>();
        _ct =CancellationToken.None;
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        CancelTimer.Map(builder);

        builder.GetEndpoint("/{gameId}")
            .ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("cancelTimerApi")
            .ShouldHaveSummaryAndDescription("Cancels the timer for game");
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        const string gameId = "game-id";
        var error = Result.Fail<TimerState>(ErrorKind.Invalid, "error code", "error message");
        _mockTimerService.Setup(o => o.CancelTimer(gameId,_ct)).ReturnsAsync(error);

        var result = await CancelTimer.Handle(gameId, _mockTimerService.Object,_ct);

        _mockTimerService.Verify(o => o.CancelTimer(gameId,_ct), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }


    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        const string gameId = "game-id";
        var error = Result.Fail<TimerState>(ErrorKind.NotFound, "error code", "error message");
        _mockTimerService.Setup(o =>o.CancelTimer(gameId,_ct)).ReturnsAsync(error);

        var result = await CancelTimer.Handle(gameId, _mockTimerService.Object,_ct);

        _mockTimerService.Verify(o => o.CancelTimer(gameId,_ct), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string gameId = "game-id";
        var success = Result.Ok(new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.None,
            ServerNowUtc = default,
            EndUtc = null,
            Label = null
        });
        _mockTimerService.Setup(o => o.CancelTimer(gameId,_ct)).ReturnsAsync(success);

        var result = await CancelTimer.Handle(gameId, _mockTimerService.Object,_ct);

        _mockTimerService.Verify(o => o.CancelTimer(gameId,_ct), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<TimerState>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}