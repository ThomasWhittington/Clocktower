using Clocktower.Server.Timer.Endpoints;
using Clocktower.Server.Timer.Services;

namespace Clocktower.ServerTests.Timer.Endpoints;

[TestClass]
public class GetTimerTests
{
    private Mock<ITimerService> _mockTimerService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockTimerService = StrictMockFactory.Create<ITimerService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetTimer.Map(builder);

        builder.GetEndpoint("/{gameId}")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("getTimerApi")
            .ShouldHaveSummaryAndDescription("Gets the timer state for game");
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string gameId = "game-id";
        var timer = new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.None,
            ServerNowUtc = default,
            EndUtc = null,
            Label = null
        };
        _mockTimerService.Setup(o => o.GetTimer(gameId)).Returns(timer);

        var result = GetTimer.Handle(gameId, _mockTimerService.Object);

        _mockTimerService.Verify(o => o.GetTimer(gameId), Times.Once);
        var response = result.Should().BeOfType<Ok<TimerState>>().Subject;
        response.Value.Should().Be(timer);
    }
}