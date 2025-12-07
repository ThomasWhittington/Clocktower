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
    [DataRow(SetMuteAllPlayersOutcome.InvalidGuildError)]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError(SetMuteAllPlayersOutcome inviteUserOutcome)
    {
        const string responseMessage = "response message";
        const string gameId = "game-id";
        const bool muted = false;
        _mockDiscordGameActionService.Setup(o => o.SetMuteAllPlayersAsync(gameId.Trim(), muted)).ReturnsAsync((inviteUserOutcome, responseMessage));

        var result = await SetMuteAllPlayers.Handle(gameId, muted, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SetMuteAllPlayersAsync(gameId.Trim(), muted), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }


    [TestMethod]
    [DataRow(SetMuteAllPlayersOutcome.GameDoesNotExistError)]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError(SetMuteAllPlayersOutcome inviteUserOutcome)
    {
        const string responseMessage = "response message";
        const string gameId = "game-id";
        const bool muted = false;
        _mockDiscordGameActionService.Setup(o => o.SetMuteAllPlayersAsync(gameId.Trim(), muted)).ReturnsAsync((inviteUserOutcome, responseMessage));

        var result = await SetMuteAllPlayers.Handle(gameId, muted, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SetMuteAllPlayersAsync(gameId.Trim(), muted), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string responseMessage = "response message";
        const string gameId = "game-id";
        const bool muted = false;
        _mockDiscordGameActionService.Setup(o => o.SetMuteAllPlayersAsync(gameId.Trim(), muted)).ReturnsAsync((SetMuteAllPlayersOutcome.PlayersUpdated, responseMessage));

        var result = await SetMuteAllPlayers.Handle(gameId, muted, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SetMuteAllPlayersAsync(gameId.Trim(), muted), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}