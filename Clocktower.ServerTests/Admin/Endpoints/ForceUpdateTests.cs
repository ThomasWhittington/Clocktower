using Clocktower.Server.Admin.Endpoints;
using Clocktower.Server.Admin.Services;
using Clocktower.Server.Discord.Town.Endpoints.Validation;

namespace Clocktower.ServerTests.Admin.Endpoints;

[TestClass]
public class ForceUpdateTests
{
    private Mock<IAdminService> _mockAdminService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockAdminService = StrictMockFactory.Create<IAdminService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        ForceUpdate.Map(builder);

        builder.GetEndpoint("/force-update/{gameId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("forceUpdateApi")
            .ShouldHaveSummaryAndDescription("Forces a game update to be sent to all users")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsInvalid()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockAdminService.Setup(o => o.ForceUpdate(request.GameId)).ReturnsAsync(error);

        var result = await ForceUpdate.Handle(request, _mockAdminService.Object);

        _mockAdminService.Verify(o => o.ForceUpdate(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFound()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockAdminService.Setup(o => o.ForceUpdate(request.GameId)).ReturnsAsync(error);

        var result = await ForceUpdate.Handle(request, _mockAdminService.Object);

        _mockAdminService.Verify(o => o.ForceUpdate(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsOk()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());
        var success = Result.Ok("success");

        _mockAdminService.Setup(o => o.ForceUpdate(request.GameId)).ReturnsAsync(success);

        var result = await ForceUpdate.Handle(request, _mockAdminService.Object);

        _mockAdminService.Verify(o => o.ForceUpdate(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}