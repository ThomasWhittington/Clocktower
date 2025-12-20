using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetGamesTests
{
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;

    private void MockResponse(GamePerspective[] allGames, GamePerspective[] guildGames)
    {
        _mockGamePerspectiveService.Setup(o => o.GetGames()).Returns(allGames);
        _mockGamePerspectiveService.Setup(o => o.GetGuildGames(It.IsAny<string>())).Returns(guildGames);
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
            .ShouldHaveSummaryAndDescription("Gets all games, optionally filtered by guildId");
    }

    [TestMethod]
    public void Handle_ReturnsOkAllGames_WhenGuildIdNull()
    {
        const string guildId = null!;
        var allGames = new[]
        {
            CommonMethods.GetGamePerspective(),
            CommonMethods.GetGamePerspective()
        };

        var guildGames = new[]
        {
            CommonMethods.GetGamePerspective()
        };

        MockResponse(allGames, guildGames);

        var result = GetGames.Handle(_mockGamePerspectiveService.Object, guildId);

        _mockGamePerspectiveService.Verify(o => o.GetGames(), Times.Once);
        _mockGamePerspectiveService.Verify(o => o.GetGuildGames(It.IsAny<string>()), Times.Never);

        var response = result.Should().BeOfType<Ok<IEnumerable<GamePerspective>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(allGames);
    }

    [TestMethod]
    public void Handle_ReturnsOkGuildGames_WhenGuildIdProvided()
    {
        var guildId = CommonMethods.GetRandomString();
        var allGames = new[]
        {
            CommonMethods.GetGamePerspective(),
            CommonMethods.GetGamePerspective()
        };

        var guildGames = new[]
        {
            CommonMethods.GetGamePerspective()
        };

        MockResponse(allGames, guildGames);

        var result = GetGames.Handle(_mockGamePerspectiveService.Object, guildId);

        _mockGamePerspectiveService.Verify(o => o.GetGames(), Times.Never);
        _mockGamePerspectiveService.Verify(o => o.GetGuildGames(It.IsAny<string>()), Times.Once);

        var response = result.Should().BeOfType<Ok<IEnumerable<GamePerspective>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(guildGames);
    }
}