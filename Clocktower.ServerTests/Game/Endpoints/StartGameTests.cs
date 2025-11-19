using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class StartGameTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;
    private const string ResponseMessage = "Response";

    private static StartGame.Request GetRandomRequest() => new(
        CommonMethods.GetRandomStringId(),
        CommonMethods.GetRandomSnowflakeId(),
        CommonMethods.GetRandomSnowflakeId()
    );

    private void MockResponse(bool success, GameState? gameState)
    {
        _mockGameStateService.Setup(o =>
                o.StartNewGame(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ulong>()))
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

        StartGame.Map(builder);

        var endpoint = builder.GetEndpoint("/{gameId}/start/{guildId}/{userId}");

        endpoint.ShouldHaveMethod(HttpMethod.Post);
        endpoint.ShouldHaveOperationId("startGameApi");
        endpoint.ShouldHaveSummaryAndDescription("Starts new game state for id");
        endpoint.ShouldHaveValidation();
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceStartNewGameReturnsFalse()
    {
        var request = GetRandomRequest();
        MockResponse(false, null);

        var result = StartGame.Handle(request, _mockGameStateService.Object);

        _mockGameStateService.Verify(o=>o.StartNewGame(request.GuildId, request.GameId.Trim(), ulong.Parse(request.UserId)),Times.Once);
        
        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(ResponseMessage);
        
    }

    [TestMethod]
    public void Handle_ReturnsCreated_WhenServiceStartNewGameReturnsTrue()
    {
        var request = GetRandomRequest();
        var gameState = new GameState
        {
            Id = CommonMethods.GetRandomStringId()
        };
        MockResponse(true, gameState);

        var result = StartGame.Handle(request, _mockGameStateService.Object);

        _mockGameStateService.Verify(o=>o.StartNewGame(request.GuildId, request.GameId.Trim(), ulong.Parse(request.UserId)),Times.Once);
        
        var response = result.Result.Should().BeOfType<Created<GameState>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        response.Location.Should().Be($"/games/{gameState.Id}");
        response.Value.Should().Be(gameState);
    }
}