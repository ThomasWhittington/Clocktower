using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetDraftRolesTests
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

        SetDraftRoles.Map(builder);

        builder.GetEndpoint("/{gameId}/set-draft-roles")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setDraftRolesApi")
            .ShouldHaveSummaryAndDescription("Sets draft roles for multiple players")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsInvalidError()
    {
        var dictionary = new Dictionary<string, string>
        {
            { "userId1", "roleId1" },
            { "userId2", "roleId2" },
            { "userId3", "roleId3" }
        };
        var body = new SetDraftRoles.Body(dictionary);
        var request = new SetDraftRoles.Request(CommonMethods.GetRandomString(), body);
        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");

        _mockGameService.Setup(o => o.SetDraftRoles(request.GameId, request.Body.PlayerRoles)).ReturnsAsync(error);

        var result = await SetDraftRoles.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetDraftRoles(request.GameId, request.Body.PlayerRoles), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        var dictionary = new Dictionary<string, string>
        {
            { "userId1", "roleId1" },
            { "userId2", "roleId2" },
            { "userId3", "roleId3" }
        };
        var body = new SetDraftRoles.Body(dictionary);
        var request = new SetDraftRoles.Request(CommonMethods.GetRandomString(), body);
        var error = Result.Fail<string>(ErrorKind.NotFound, "error code", "error message");

        _mockGameService.Setup(o => o.SetDraftRoles(request.GameId, request.Body.PlayerRoles)).ReturnsAsync(error);

        var result = await SetDraftRoles.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetDraftRoles(request.GameId, request.Body.PlayerRoles), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsOk()
    {
        var dictionary = new Dictionary<string, string>
        {
            { "userId1", "roleId1" },
            { "userId2", "roleId2" },
            { "userId3", "roleId3" }
        };
        var body = new SetDraftRoles.Body(dictionary);
        var request = new SetDraftRoles.Request(CommonMethods.GetRandomString(), body);
        var success = Result.Ok("success");

        _mockGameService.Setup(o => o.SetDraftRoles(request.GameId, request.Body.PlayerRoles)).ReturnsAsync(success);

        var result = await SetDraftRoles.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SetDraftRoles(request.GameId, request.Body.PlayerRoles), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}