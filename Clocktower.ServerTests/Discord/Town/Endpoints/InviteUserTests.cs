using Clocktower.Server.Data.Types;
using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class InviteUserTests
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

        InviteUser.Map(builder);

        builder.GetEndpoint("/{gameId}/invite/{userId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("inviteUserApi")
            .ShouldHaveSummaryAndDescription("Invites user to the specified game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    [DataRow(InviteUserOutcome.InvalidGuildError)]
    [DataRow(InviteUserOutcome.DmChannelError)]
    [DataRow(InviteUserOutcome.UnknownError)]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError(InviteUserOutcome inviteUserOutcome)
    {
        const string responseMessage = "response message";
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.InviteUser(request.GameId.Trim(), request.UserId)).ReturnsAsync((inviteUserOutcome, responseMessage));

        var result = await InviteUser.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.InviteUser(request.GameId.Trim(), request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    [DataRow(InviteUserOutcome.GameDoesNotExistError)]
    [DataRow(InviteUserOutcome.UserNotFoundError)]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError(InviteUserOutcome inviteUserOutcome)
    {
        const string responseMessage = "response message";
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.InviteUser(request.GameId.Trim(), request.UserId)).ReturnsAsync((inviteUserOutcome, responseMessage));

        var result = await InviteUser.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.InviteUser(request.GameId.Trim(), request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string responseMessage = "response message";
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.InviteUser(request.GameId.Trim(), request.UserId)).ReturnsAsync((InviteUserOutcome.InviteSent, responseMessage));

        var result = await InviteUser.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.InviteUser(request.GameId.Trim(), request.UserId), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}