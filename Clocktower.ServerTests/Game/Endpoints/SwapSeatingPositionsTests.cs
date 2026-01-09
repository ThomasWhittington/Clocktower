using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class SwapSeatingPositionsTests
{
    private Mock<IGameService> _mockGameService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGameService = new Mock<IGameService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        SwapSeatingPositions.Map(builder);

        builder.GetEndpoint("/{gameId}/swap-seating-positions/{userId1}/{userId2}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("swapSeatingPositionsApi")
            .ShouldHaveSummaryAndDescription("Swaps the seats for two players in the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new SwapSeatingPositions.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId());

        var error = Result.Fail<string>(ErrorKind.Invalid, "error code", "error message");
        _mockGameService.Setup(o => o.SwapSeatingPositions(request.GameId, request.UserId1, request.UserId2)).ReturnsAsync(error);

        var result = await SwapSeatingPositions.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SwapSeatingPositions(request.GameId, request.UserId1, request.UserId2), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new SwapSeatingPositions.Request(CommonMethods.GetRandomString(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId());

        var success = Result.Ok("expected");

        _mockGameService.Setup(o => o.SwapSeatingPositions(request.GameId, request.UserId1, request.UserId2)).ReturnsAsync(success);

        var result = await SwapSeatingPositions.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.SwapSeatingPositions(request.GameId, request.UserId1, request.UserId2), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}