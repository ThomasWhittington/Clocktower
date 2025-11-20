using Clocktower.Server.Common.Api.Auth;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Common.Api.Auth;

[TestClass]
public class GameAuthorizationServiceTests
{
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private IGameAuthorizationService Sut => new GameAuthorizationService(_mockGameStateStore.Object);


    private void MockResponse(string gameId, GameState gameState)
    {
        _mockGameStateStore.Setup(o =>
                o.Get(gameId))
            .Returns(gameState);
    }


    [TestInitialize]
    public void Setup()
    {
        _mockGameStateStore = new Mock<IGameStateStore>();
    }


    private static GameState CreateGameState(string gameId, List<(string userId, UserType userType)> users)
    {
        var gameUsers = new List<GameUser>();

        foreach ((string userId, UserType userType) in users)
        {
            gameUsers.Add(new GameUser(userId, It.IsAny<string>(), It.IsAny<string>())
            {
                UserType = userType
            });
        }

        return new GameState
        {
            Id = gameId,
            Users = gameUsers
        };
    }


    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenUnknownGameId()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gameState = CreateGameState("dummy", [new ValueTuple<string, UserType>(userId, UserType.StoryTeller)]);
        MockResponse("dummy", gameState);
        
        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnFalse_WhenUnknownUserId()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gameState = new GameState { Id = gameId };
        MockResponse(gameId, gameState);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenUnknownUserIdUnknownGameId()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gameState = new GameState { Id = "dummy" };
        MockResponse(gameId, gameState);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenPlayerNotStoryTeller()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gameState = CreateGameState(gameId, [new ValueTuple<string, UserType>(userId, UserType.Player)]);
        MockResponse(gameId, gameState);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsTrue_WhenPlayerIsStoryTeller()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gameState = CreateGameState(gameId, [new ValueTuple<string, UserType>(userId, UserType.StoryTeller)]);
        MockResponse(gameId, gameState);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeTrue();
    }
}