using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetPerspectiveRoleTests
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

        SetPerspectiveRole.Map(builder);

        builder.GetEndpoint("/{gameId}/set-perspective-role/{userId}/{targetUserId}/{roleId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("setPerspectiveRoleApi")
            .ShouldHaveSummaryAndDescription("Sets the role for the target user on the users perspective of the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new SetPerspectiveRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetPerspectiveRole(request.GameId, request.UserId, request.TargetUserId, request.RoleId)).ReturnsAsync(error);

        var result = await SetPerspectiveRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetPerspectiveRole(request.GameId, request.UserId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsFalse()
    {
        var request = new SetPerspectiveRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.SetPerspectiveRole(request.GameId, request.UserId, request.TargetUserId, request.RoleId)).ReturnsAsync(error);

        var result = await SetPerspectiveRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetPerspectiveRole(request.GameId, request.UserId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new SetPerspectiveRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.SetPerspectiveRole(request.GameId, request.UserId, request.TargetUserId, request.RoleId)).ReturnsAsync(success);

        var result = await SetPerspectiveRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetPerspectiveRole(request.GameId, request.UserId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}