using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class SetTimeTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockGameStateService = new Mock<IGameStateService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SetTime.Map(builder);

        builder.GetEndpoint("/{gameId}/time")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("setTimeApi")
            .ShouldHaveSummary("Sets the time of the town")
            .ShouldHaveDescription("Sets the game state of the town based on the day time");
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new SetTime.Request(CommonMethods.GetRandomString(), GameTime.Day);

        _mockGameStateService.Setup(o => o.SetTime(request.GameId.Trim(), request.GameTime)).ReturnsAsync((false, responseMessage));

        var result = await SetTime.Handle(request, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.SetTime(request.GameId.Trim(), request.GameTime), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new SetTime.Request(CommonMethods.GetRandomString(), GameTime.Day);

        _mockGameStateService.Setup(o => o.SetTime(request.GameId.Trim(), request.GameTime)).ReturnsAsync((true, responseMessage));

        var result = await SetTime.Handle(request, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.SetTime(request.GameId.Trim(), request.GameTime), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}