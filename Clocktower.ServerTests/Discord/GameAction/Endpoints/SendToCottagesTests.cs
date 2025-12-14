using Clocktower.Server.Discord.GameAction.Endpoints;
using Clocktower.Server.Discord.GameAction.Services;
using Clocktower.Server.Discord.Town.Endpoints.Validation;

namespace Clocktower.ServerTests.Discord.GameAction.Endpoints;

[TestClass]
public class SendToCottagesTests
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

        SendToCottages.Map(builder);

        builder.GetEndpoint("/send-to-cottages/{gameId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("sendToCottagesApi")
            .ShouldHaveSummary("Sends all users to cottages")
            .ShouldHaveDescription("Sends all users to cottages and storytellers to consultation")
            .ShouldHaveValidation();
    }

    [TestMethod]
    [DataRow(GameActionOutcome.InvalidGuildError)]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError(GameActionOutcome gameActionOutcome)
    {
        const string gameId = "game-id";
        var request = new GameIdRequest(gameId);
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        _mockDiscordGameActionService.Setup(o => o.SendToCottagesAsync(gameId)).ReturnsAsync(error);

        var result = await SendToCottages.Handle(request, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SendToCottagesAsync(gameId), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }


    [TestMethod]
    [DataRow(GameActionOutcome.GameDoesNotExistError)]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError(GameActionOutcome gameActionOutcome)
    {
        const string gameId = "game-id";
        var request = new GameIdRequest(gameId);
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");
        _mockDiscordGameActionService.Setup(o => o.SendToCottagesAsync(gameId)).ReturnsAsync(error);

        var result = await SendToCottages.Handle(request, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SendToCottagesAsync(gameId), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string gameId = "game-id";
        var request = new GameIdRequest(gameId);
        var success = Result.Ok("response message");
        _mockDiscordGameActionService.Setup(o => o.SendToCottagesAsync(gameId)).ReturnsAsync(success);

        var result = await SendToCottages.Handle(request, _mockDiscordGameActionService.Object);

        _mockDiscordGameActionService.Verify(o => o.SendToCottagesAsync(gameId), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}