using Clocktower.Server.Data;
using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Endpoints;

[TestClass]
public class GetGamesTests
{
    private Mock<IGameStateService> _mockGameStateService = null!;

    private void MockResponse(GameState[] allGames, GameState[] guildGames)
    {
        _mockGameStateService.Setup(o => o.GetGames()).Returns(allGames);
        _mockGameStateService.Setup(o => o.GetGuildGames(It.IsAny<string>())).Returns(guildGames);
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
        var allGames = new GameState[]
        {
            new() { Id = CommonMethods.GetRandomString() },
            new() { Id = CommonMethods.GetRandomString() },
        };

        var guildGames = new GameState[]
        {
            new() { Id = CommonMethods.GetRandomString() }
        };

        MockResponse(allGames, guildGames);

        var result = GetGames.Handle(_mockGameStateService.Object, guildId);

        _mockGameStateService.Verify(o => o.GetGames(), Times.Once);
        _mockGameStateService.Verify(o => o.GetGuildGames(It.IsAny<string>()), Times.Never);

        var response = result.Should().BeOfType<Ok<IEnumerable<GameState>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(allGames);
    }

    [TestMethod]
    public void Handle_ReturnsOkGuildGames_WhenGuildIdProvided()
    {
        var guildId = CommonMethods.GetRandomString();
        var allGames = new GameState[]
        {
            new() { Id = CommonMethods.GetRandomString() },
            new() { Id = CommonMethods.GetRandomString() },
        };

        var guildGames = new GameState[]
        {
            new() { Id = CommonMethods.GetRandomString() }
        };

        MockResponse(allGames, guildGames);

        var result = GetGames.Handle(_mockGameStateService.Object, guildId);

        _mockGameStateService.Verify(o => o.GetGames(), Times.Never);
        _mockGameStateService.Verify(o => o.GetGuildGames(It.IsAny<string>()), Times.Once);

        var response = result.Should().BeOfType<Ok<IEnumerable<GameState>>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().BeEquivalentTo(guildGames);
    }
}