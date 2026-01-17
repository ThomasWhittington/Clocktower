using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetDraftRoleTests
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

        SetDraftRole.Map(builder);

        builder.GetEndpoint("/{gameId}/set-draft-role/{targetUserId}/{roleId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setDraftRoleApi")
            .ShouldHaveSummaryAndDescription("Sets the draft role for the target user")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new SetDraftRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId)).ReturnsAsync(error);

        var result = await SetDraftRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsFalse()
    {
        var request = new SetDraftRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId)).ReturnsAsync(error);

        var result = await SetDraftRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new SetDraftRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId)).ReturnsAsync(success);

        var result = await SetDraftRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}