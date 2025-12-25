using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetPlayerGamesTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;

    private void MockResponse(string userId, MiniGamePerspective[] playerGames)
    {
        _mockGamePerspectiveService.Setup(o => o.GetPlayerGames(userId)).Returns(playerGames);
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

        GetPlayerGames.Map(builder);

        builder.GetEndpoint("/player/{userId}")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getPlayerGamesApi")
            .ShouldHaveSummaryAndDescription("Gets games the player is in");
    }

    [TestMethod]
    public void Handle_ReturnsOkPlayerGamesGames()
    {
        var userId = CommonMethods.GetRandomString();
        var playerGames = new MiniGamePerspective[]
        {
            new(CommonMethods.GetRandomString(), CommonMethods.GetRandomGameUser(), DateTime.UtcNow),
        };

        MockResponse(userId, playerGames);

        var result = GetPlayerGames.Handle(userId, _mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetPlayerGames(userId.Trim()), Times.Once);

        var response = result.Should().BeOfType<Ok<IEnumerable<MiniGamePerspective>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(playerGames);
    }
}