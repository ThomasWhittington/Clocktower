using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetGamesTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;

    private void MockResponse(GamePerspective[] allGames)
    {
        _mockGamePerspectiveService.Setup(o => o.GetGames()).Returns(allGames);
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

        GetGames.Map(builder);

        builder.GetEndpoint("/")
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

        var result = GetGames.Handle(_mockGamePerspectiveService.Object);

        _mockGamePerspectiveService.Verify(o => o.GetGames(), Times.Once);

        var response = result.Should().BeOfType<Ok<IEnumerable<GamePerspective>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(allGames);
    }
}