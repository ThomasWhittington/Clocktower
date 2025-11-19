using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetPlayerGamesTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;

    private void MockResponse(string userId, MiniGameState[] playerGames)
    {
        _mockGameStateService.Setup(o => o.GetPlayerGames(userId)).Returns(playerGames);
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

        GetPlayerGames.Map(builder);

        var endpoint = builder.GetEndpoint("/player/{userId}");

        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("getPlayerGamesApi");
        endpoint.ShouldHaveSummaryAndDescription("Gets games the player is in");
    }

    [TestMethod]
    public void Handle_ReturnsOkPlayerGamesGames()
    {
        var userId = CommonMethods.GetRandomStringId();
        var playerGames = new MiniGameState[]
        {
            new(CommonMethods.GetRandomStringId(), new GameUser(
                CommonMethods.GetRandomStringId(),
                CommonMethods.GetRandomStringId(),
                CommonMethods.GetRandomStringId()
            ), DateTime.UtcNow),
        };

        MockResponse(userId, playerGames);

        var result = GetPlayerGames.Handle(userId, _mockGameStateService.Object);

        _mockGameStateService.Verify(o => o.GetPlayerGames(userId.Trim()), Times.Once);

        var response = result.Should().BeOfType<Ok<IEnumerable<MiniGameState>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(playerGames);
    }
}