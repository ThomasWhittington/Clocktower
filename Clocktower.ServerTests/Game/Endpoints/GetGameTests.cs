using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetGameTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;
    private const string ResponseMessage = "Response";

    private void MockResponse(bool success, GamePerspective? gamePerspective)
    {
        _mockGamePerspectiveService.Setup(o =>
                o.GetGame(It.IsAny<string>()))
            .Returns((success, gamePerspective is null ? [] : [gamePerspective], ResponseMessage));
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveService = new Mock<IGamePerspectiveService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetGame.Map(builder);

        builder.GetEndpoint("/{gameId}")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getGameApi")
            .ShouldHaveSummaryAndDescription("Get the game perspective by id");
    }

    [TestMethod]
    public void Handle_ReturnsNotFound_WhenServiceGetGameReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomString();
        MockResponse(false, null);

        var result = GetGame.Handle(gameId, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetGame(gameId.Trim()), Times.Once);

        var response = result.Result.Should().BeOfType<NotFound<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        response.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceGetGameReturnsTrue()
    {
        var gameId = CommonMethods.GetRandomString();
        var gamePerspective = CommonMethods.GetGamePerspective();
        MockResponse(true, gamePerspective);

        var result = GetGame.Handle(gameId, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetGame(gameId.Trim()), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<IEnumerable<GamePerspective>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo([gamePerspective]);
    }
}