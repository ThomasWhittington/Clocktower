using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class RandomiseSeatingPositionsTests
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

        RandomiseSeatingPositions.Map(builder);

        builder.GetEndpoint("/{gameId}/randomise-seating-positions")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveStorytellerAuthorization()
            .ShouldHaveOperationId("randomiseSeatingPositionsApi")
            .ShouldHaveSummaryAndDescription("Randomises seating positions for players in the game")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());

        var error = Result.Fail<string[]>(ErrorKind.Invalid, "error code", "error message");
        _mockGameService.Setup(o => o.RandomiseSeatingPositions(request.GameId)).ReturnsAsync(error);

        var result = await RandomiseSeatingPositions.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.RandomiseSeatingPositions(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<ErrorResponse>>().Subject;
        response.Value.ShouldBeError(error);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        var request = new GameIdRequest(CommonMethods.GetRandomString());

        var success = Result.Ok(new[] { "expected1", "expected2" });

        _mockGameService.Setup(o => o.RandomiseSeatingPositions(request.GameId)).ReturnsAsync(success);

        var result = await RandomiseSeatingPositions.Handle(request, _mockGameService.Object);

        _mockGameService.Verify(o => o.RandomiseSeatingPositions(request.GameId), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<string[]>>().Subject;
        response.Value.Should().BeEquivalentTo(success.Value);
    }
}