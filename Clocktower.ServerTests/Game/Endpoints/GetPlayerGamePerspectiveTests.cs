using Clocktower.Server.Data;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetPlayerGamePerspectiveTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveService = StrictMockFactory.Create<IGamePerspectiveService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetPlayerGamePerspective.Map(builder);

        builder.GetEndpoint("/{gameId}/players/{userId}/state")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("getPlayerGamePerspectiveApi")
            .ShouldHaveSummary("Get the player game perspective")
            .ShouldHaveDescription("Gets the game perspective that the provided user is allowed to see")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        const string gameId = "game-id";
        const string userId = "12345";
        var request = new GameAndUserRequest(gameId, userId);
        var error = Result.Fail<GamePerspectiveDto>(ErrorKind.Invalid, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGamePerspective(gameId, userId)).ReturnsAsync(error);

        var result = await GetPlayerGamePerspective.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGamePerspective(gameId, userId), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }


    [TestMethod]
    public async Task Handle_ReturnsNotFound_WhenServiceReturnsNotFoundError()
    {
        const string gameId = "game-id";
        const string userId = "12345";
        var request = new GameAndUserRequest(gameId, userId);
        var error = Result.Fail<GamePerspectiveDto>(ErrorKind.NotFound, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGamePerspective(gameId, userId)).ReturnsAsync(error);

        var result = await GetPlayerGamePerspective.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGamePerspective(gameId, userId), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceSendsInvite()
    {
        const string gameId = "game-id";
        const string userId = "12345";
        var request = new GameAndUserRequest(gameId, userId);
        var success = Result.Ok(CommonMethods.GetGamePerspectiveDto());
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGamePerspective(gameId, userId)).ReturnsAsync(success);

        var result = await GetPlayerGamePerspective.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGamePerspective(gameId, userId), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<GamePerspectiveDto>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}