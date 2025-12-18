using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetGameTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;
    private const string ResponseMessage = "Response";

    private void MockResponse(bool success, GameState? gameState)
    {
        _mockGameStateService.Setup(o =>
                o.GetGame(It.IsAny<string>()))
            .Returns((success, gameState, ResponseMessage));
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGameStateService = new Mock<IGameStateService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetGame.Map(builder);

        builder.GetEndpoint("/{gameId}")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getGameApi")
            .ShouldHaveSummaryAndDescription("Get the game state by id");
    }

    [TestMethod]
    public void Handle_ReturnsNotFound_WhenServiceGetGameReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomString();
        MockResponse(false, null);

        var result = GetGame.Handle(gameId, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.GetGame(gameId.Trim()), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceGetGameReturnsTrue()
    {
        var gameId = CommonMethods.GetRandomString();
        var gameState = CommonMethods.GetGameState();
        MockResponse(true, gameState);

        var result = GetGame.Handle(gameId, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.GetGame(gameId.Trim()), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<GameState>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(gameState);
    }
}