using Clocktower.Server.Data;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;
using Microsoft.Extensions.Logging;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class StartGameTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;
    private Mock<IDiscordTownService> _mockDiscordTownService = null!;
    private Mock<ILogger<StartGame>> _mockLogger = null!;
    private const string ResponseMessage = "Response";

    private static StartGame.Request GetRandomRequest() => new(
        CommonMethods.GetRandomString(),
        CommonMethods.GetRandomSnowflakeStringId(),
        CommonMethods.GetRandomSnowflakeStringId()
    );

    private void MockResponse(bool success, GameState? gameState, bool getTownSuccess)
    {
        _mockGameStateService.Setup(o =>
                o.StartNewGame(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((success, gameState, ResponseMessage));
        _mockDiscordTownService.Setup(o => o.GetDiscordTown(It.IsAny<string>())).ReturnsAsync((getTownSuccess, null, "message"));
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGameStateService = StrictMockFactory.Create<IGameStateService>();
        _mockDiscordTownService = StrictMockFactory.Create<IDiscordTownService>();
        _mockLogger = new Mock<ILogger<StartGame>>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        StartGame.Map(builder);

        builder.GetEndpoint("/{gameId}/start/{guildId}/{userId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("startGameApi")
            .ShouldHaveSummaryAndDescription("Starts new game state for id")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceStartNewGameReturnsFalse()
    {
        var request = GetRandomRequest();
        MockResponse(false, null, true);

        var result = await StartGame.Handle(request, _mockGameStateService.Object, _mockDiscordTownService.Object, _mockLogger.Object);

        _mockGameStateService.Verify(o => o.StartNewGame(request.GuildId, request.GameId.Trim(), request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsCreated_WhenServiceStartNewGameReturnsTrue()
    {
        var request = GetRandomRequest();
        var gameState = new GameState
        {
            Id = CommonMethods.GetRandomString()
        };
        MockResponse(true, gameState, true);

        var result = await StartGame.Handle(request, _mockGameStateService.Object, _mockDiscordTownService.Object, _mockLogger.Object);

        _mockGameStateService.Verify(o => o.StartNewGame(request.GuildId, request.GameId.Trim(), request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<Created<GameState>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        response.Location.Should().Be($"/games/{gameState.Id}");
        response.Value.Should().Be(gameState);
    }

    [TestMethod]
    public async Task Handle_LogsWarning_WhenTownNotFound()
    {
        var request = GetRandomRequest();
        var gameState = new GameState
        {
            Id = CommonMethods.GetRandomString()
        };
        MockResponse(true, gameState, false);

        var result = await StartGame.Handle(request, _mockGameStateService.Object, _mockDiscordTownService.Object, _mockLogger.Object);

        _mockGameStateService.Verify(o => o.StartNewGame(request.GuildId, request.GameId.Trim(), request.UserId), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        var response = result.Result.Should().BeOfType<Created<GameState>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.Created);
        response.Location.Should().Be($"/games/{gameState.Id}");
        response.Value.Should().Be(gameState);
    }
}