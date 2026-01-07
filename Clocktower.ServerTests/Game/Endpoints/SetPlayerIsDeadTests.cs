using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetPlayerIsDeadTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveService = new Mock<IGamePerspectiveService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SetPlayerIsDead.Map(builder);

        builder.GetEndpoint("/{gameId}/set-player-is-dead/{userId}/{isDead:bool}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setPlayerIsDeadApi")
            .ShouldHaveSummaryAndDescription("Sets a player's dead status in the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new SetPlayerIsDead.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), new Random().Next(2) == 0);

        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.SetPlayerIsDead(request.GameId, request.UserId, request.IsDead)).ReturnsAsync(error);

        var result = await SetPlayerIsDead.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.SetPlayerIsDead(request.GameId, request.UserId, request.IsDead), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new SetPlayerIsDead.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), new Random().Next(2) == 0);

        var success = Result.Ok("expected");

        _mockGamePerspectiveService.Setup(o => o.SetPlayerIsDead(request.GameId, request.UserId, request.IsDead)).ReturnsAsync(success);

        var result = await SetPlayerIsDead.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.SetPlayerIsDead(request.GameId, request.UserId, request.IsDead), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}