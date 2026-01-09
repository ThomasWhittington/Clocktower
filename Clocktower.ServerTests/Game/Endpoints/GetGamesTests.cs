using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetGamesTests
{
    private Mock<IGameService> _mockGameService = null!;

    private void MockResponse(GamePerspective[] allGames)
    {
        _mockGameService.Setup(o => o.GetGames()).Returns(allGames);
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGameService = new Mock<IGameService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetGames.Map(builder);

        builder.GetEndpoint("/all")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getGamesApi")
            .ShouldHaveSummaryAndDescription("Gets all games perspectives");
    }

    [TestMethod]
    public void Handle_ReturnsOkAllGames()
    {
        var allGames = new[]
        {
            CommonMethods.GetGamePerspective(),
            CommonMethods.GetGamePerspective()
        };

        MockResponse(allGames);

        var result = GetGames.Handle(_mockGameService.Object);

        _mockGameService.Verify(o => o.GetGames(), Times.Once);

        var response = result.Should().BeOfType<Ok<IEnumerable<GamePerspective>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(allGames);
    }
}