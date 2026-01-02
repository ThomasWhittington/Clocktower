using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class AddUserToGameTests
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

        AddUserToGame.Map(builder);

        builder.GetEndpoint("/{gameId}/add-user/{userId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("addUserToGameApi")
            .ShouldHaveSummaryAndDescription("Adds user to the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomString());

        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.AddUserToGame(request.GameId, request.UserId)).ReturnsAsync(error);

        var result = await AddUserToGame.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.AddUserToGame(request.GameId, request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new GameAndUserRequest(CommonMethods.GetRandomString(), CommonMethods.GetRandomString());

        var success = Result.Ok("expected");

        _mockGamePerspectiveService.Setup(o => o.AddUserToGame(request.GameId, request.UserId)).ReturnsAsync(success);

        var result = await AddUserToGame.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.AddUserToGame(request.GameId, request.UserId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().Be(success.Value);
    }
}