using Clocktower.Server.Discord.GameAction.Endpoints;
using Clocktower.Server.Discord.GameAction.Services;

namespace Clocktower.ServerTests.Discord.GameAction.Endpoints;

[TestClass]
public class SetMuteAllPlayersTests
{
    private Mock<IDiscordGameActionService> _mockDiscordGameActionService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordGameActionService = new Mock<IDiscordGameActionService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SetMuteAllPlayers.Map(builder);

        builder.GetEndpoint("/set-mute-players/{gameId}/{muted:bool}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setMuteAllPlayersApi")
            .ShouldHaveSummary("Sets muted status for players in game")
            .ShouldHaveDescription("Sets muted status for players (not storytellers/ spectators) connected to voice for game");
    }


    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceFails()
    {
        const string gameId = "game-id";
        const bool muted = false;
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        _mockDiscordGameActionService.Setup(o => o.SetMuteAllPlayersAsync(gameId, muted)).ReturnsAsync(error);

        var result = await SetMuteAllPlayers.Handle(gameId, muted, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SetMuteAllPlayersAsync(gameId, muted), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }


    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        const string gameId = "game-id";
        const bool muted = false;
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");
        _mockDiscordGameActionService.Setup(o => o.SetMuteAllPlayersAsync(gameId, muted)).ReturnsAsync(error);

        var result = await SetMuteAllPlayers.Handle(gameId, muted, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SetMuteAllPlayersAsync(gameId, muted), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string gameId = "game-id";
        const bool muted = false;
        var success = Result.Ok("response message");
        _mockDiscordGameActionService.Setup(o => o.SetMuteAllPlayersAsync(gameId, muted)).ReturnsAsync(success);

        var result = await SetMuteAllPlayers.Handle(gameId, muted, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SetMuteAllPlayersAsync(gameId, muted), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(success.Value);
    }
}