using Clocktower.Server.Data;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetPlayerGameStateTests
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

        GetPlayerGameState.Map(builder);

        builder.GetEndpoint("/{gameId}/players/{userId}/state")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("getPlayerGameStateApi")
            .ShouldHaveSummary("Get the player game state")
            .ShouldHaveDescription("Gets the game state that the provided user is allowed to see")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsBadRequestError()
    {
        const string gameId = "game-id";
        const string userId = "12345";
        var request = new GameAndUserRequest(gameId, userId);
        var error = Result.Fail<GamePerspectiveDto>(ErrorKind.Invalid, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGameState(gameId, userId)).ReturnsAsync(error);

        var result = await GetPlayerGameState.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGameState(gameId, userId), Times.Once);
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
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGameState(gameId, userId)).ReturnsAsync(error);

        var result = await GetPlayerGameState.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGameState(gameId, userId), Times.Once);
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
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGameState(gameId, userId)).ReturnsAsync(success);

        var result = await GetPlayerGameState.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGameState(gameId, userId), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<GamePerspectiveDto>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}