using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetReminderTests
{
    private Mock<IGameService> _mockGameService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockGameService = new Mock<IGameService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SetReminder.Map(builder);

        builder.GetEndpoint("/{gameId}/set-reminder/{targetUserId}/{reminderId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("setReminderApi")
            .ShouldHaveSummaryAndDescription("Set a reminder for a player in a game")
            .ShouldHaveValidation()
            .ShouldRequireAuthenticatedUser();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsInvalidError()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var user = CommonMethods.CreateClaimsPrincipal(userId);
        var request = new SetReminder.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetReminder(request.GameId, userId, request.TargetUserId, request.ReminderId)).ReturnsAsync(error);

        var result = await SetReminder.Handle(user, request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetReminder(request.GameId, userId, request.TargetUserId, request.ReminderId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var user = CommonMethods.CreateClaimsPrincipal(userId);
        var request = new SetReminder.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.SetReminder(request.GameId, userId, request.TargetUserId, request.ReminderId)).ReturnsAsync(error);

        var result = await SetReminder.Handle(user, request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetReminder(request.GameId, userId, request.TargetUserId, request.ReminderId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsOk()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var user = CommonMethods.CreateClaimsPrincipal(userId);
        var request = new SetReminder.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.SetReminder(request.GameId, userId, request.TargetUserId, request.ReminderId)).ReturnsAsync(success);

        var result = await SetReminder.Handle(user, request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetReminder(request.GameId, userId, request.TargetUserId, request.ReminderId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}
