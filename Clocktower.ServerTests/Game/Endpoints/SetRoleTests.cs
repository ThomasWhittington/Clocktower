using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetRoleTests
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

        SetRole.Map(builder);

        builder.GetEndpoint("/{gameId}/set-role/{targetUserId}/{roleId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setRoleApi")
            .ShouldHaveSummaryAndDescription("Sets the role for the target user on all perspectives")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new SetRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetRole(request.GameId, request.TargetUserId, request.RoleId)).ReturnsAsync(error);

        var result = await SetRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetRole(request.GameId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsFalse()
    {
        var request = new SetRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.SetRole(request.GameId, request.TargetUserId, request.RoleId)).ReturnsAsync(error);

        var result = await SetRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetRole(request.GameId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new SetRole.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.SetRole(request.GameId, request.TargetUserId, request.RoleId)).ReturnsAsync(success);

        var result = await SetRole.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetRole(request.GameId, request.TargetUserId, request.RoleId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}