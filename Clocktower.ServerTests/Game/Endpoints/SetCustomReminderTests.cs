using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetCustomReminderTests
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

        SetCustomReminder.Map(builder);

        builder.GetEndpoint("/{gameId}/set-custom-reminder/{userId}/{targetUserId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("setCustomReminderApi")
            .ShouldHaveSummaryAndDescription("Sets a custom, free-text reminder for a player in a game, visible only to the user who set it")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsInvalidError()
    {
        var request = new SetCustomReminder.Request(
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomSnowflakeStringId(),
            CommonMethods.GetRandomSnowflakeStringId(),
            new SetCustomReminder.Body(CommonMethods.GetRandomString()));
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetCustomReminder(request.GameId, request.UserId, request.TargetUserId, request.Body.ReminderText)).ReturnsAsync(error);

        var result = await SetCustomReminder.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetCustomReminder(request.GameId, request.UserId, request.TargetUserId, request.Body.ReminderText), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        var request = new SetCustomReminder.Request(
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomSnowflakeStringId(),
            CommonMethods.GetRandomSnowflakeStringId(),
            new SetCustomReminder.Body(CommonMethods.GetRandomString()));
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.SetCustomReminder(request.GameId, request.UserId, request.TargetUserId, request.Body.ReminderText)).ReturnsAsync(error);

        var result = await SetCustomReminder.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetCustomReminder(request.GameId, request.UserId, request.TargetUserId, request.Body.ReminderText), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsOk()
    {
        var request = new SetCustomReminder.Request(
            CommonMethods.GetRandomString(),
            CommonMethods.GetRandomSnowflakeStringId(),
            CommonMethods.GetRandomSnowflakeStringId(),
            new SetCustomReminder.Body(CommonMethods.GetRandomString()));
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.SetCustomReminder(request.GameId, request.UserId, request.TargetUserId, request.Body.ReminderText)).ReturnsAsync(success);

        var result = await SetCustomReminder.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetCustomReminder(request.GameId, request.UserId, request.TargetUserId, request.Body.ReminderText), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}
