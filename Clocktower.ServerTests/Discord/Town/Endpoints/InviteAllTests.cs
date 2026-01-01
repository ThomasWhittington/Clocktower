using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class InviteAllTests
{
    private Mock<IDiscordTownService> _mockDiscordTownService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordTownService = new Mock<IDiscordTownService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        InviteAll.Map(builder);

        builder.GetEndpoint("/{gameId}/invite-all")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("inviteAllApi")
            .ShouldHaveSummaryAndDescription("Invites all users to the specified game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        const string gameId = "game-id";
        var request = new GameIdRequest(gameId);
        _mockDiscordTownService.Setup(o => o.InviteAll(request.GameId, true)).ReturnsAsync(error);

        var result = await InviteAll.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.InviteAll(request.GameId, true), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");
        const string gameId = "game-id";
        var request = new GameIdRequest(gameId);
        _mockDiscordTownService.Setup(o => o.InviteAll(request.GameId, true)).ReturnsAsync(error);

        var result = await InviteAll.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.InviteAll(request.GameId, true), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string gameId = "test";
        var request = new GameIdRequest(gameId);
        var success = Result.Ok("response message");
        _mockDiscordTownService.Setup(o => o.InviteAll(request.GameId, false)).ReturnsAsync(success);

        var result = await InviteAll.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.InviteAll(request.GameId, false), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}