using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class DeleteGameTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;
    private const string ResponseMessage = "Response";

    private void MockResponse(bool success)
    {
        _mockGameStateService.Setup(o =>
                o.DeleteGame(It.IsAny<string>()))
            .Returns((success, ResponseMessage));
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGameStateService = new Mock<IGameStateService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        DeleteGame.Map(builder);

        var endpoint = builder.GetEndpoint("/{gameId}/delete");

        endpoint.ShouldHaveMethod(HttpMethod.Delete);
        endpoint.ShouldHaveOperationId("deleteGameApi");
        endpoint.ShouldHaveSummaryAndDescription("Deletes a game by id");
    }

    [TestMethod]
    public void Handle_ReturnsNotFound_WhenServiceDeleteGameReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomStringId();
        MockResponse(false);

        var result = DeleteGame.Handle(gameId, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.DeleteGame(gameId.Trim()), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.Value.Should().Be(ResponseMessage);
    }


    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceStartNewGameReturnsTrue()
    {
        var gameId = CommonMethods.GetRandomStringId();
        MockResponse(true);

        var result = DeleteGame.Handle(gameId, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.DeleteGame(gameId.Trim()), Times.Once);

        var response = result.Result.Should().BeOfType<Ok>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}