using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SetPlayerHasVoteTokenTests
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

        SetPlayerHasVoteToken.Map(builder);

        builder.GetEndpoint("/{gameId}/set-player-has-vote-toke/{userId}/{hasVoteToken:bool}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("setPlayerHasVoteTokenApi")
            .ShouldHaveSummaryAndDescription("Sets if a player has a vote token in the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new SetPlayerHasVoteToken.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), new Random().Next(2) == 0);

        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        _mockGamePerspectiveService.Setup(o => o.SetPlayerHasVoteToken(request.GameId, request.UserId, request.HasVoteToken)).ReturnsAsync(error);

        var result = await SetPlayerHasVoteToken.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.SetPlayerHasVoteToken(request.GameId, request.UserId, request.HasVoteToken), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new SetPlayerHasVoteToken.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), new Random().Next(2) == 0);

        var success = Result.Ok("expected");

        _mockGamePerspectiveService.Setup(o => o.SetPlayerHasVoteToken(request.GameId, request.UserId, request.HasVoteToken)).ReturnsAsync(success);

        var result = await SetPlayerHasVoteToken.Handle(request, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.SetPlayerHasVoteToken(request.GameId, request.UserId, request.HasVoteToken), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}