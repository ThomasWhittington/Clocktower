using Clocktower.Server.Common.Api.Auth;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Common.Api.Auth;

[TestClass]
public class GameAuthorizationServiceTests
{
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
        string userId = CommonMethods.GetRandomStringId();
        string gameId = CommonMethods.GetRandomStringId();
        var gameState = CreateGameState("dummy", [new ValueTuple<string, UserType>(userId, UserType.StoryTeller)]);
        GameStateStore.Set("dummy", gameState);
        var sut = new GameAuthorizationService();

        var result = sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnFalse_WhenUnknownUserId()
    {
        string userId = CommonMethods.GetRandomStringId();
        string gameId = CommonMethods.GetRandomStringId();
        var gameState = new GameState { Id = gameId };
        GameStateStore.Set(gameId, gameState);
        var sut = new GameAuthorizationService();

        var result = sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenUnknownUserIdUnknownGameId()
    {
        string userId = CommonMethods.GetRandomStringId();
        string gameId = CommonMethods.GetRandomStringId();
        var gameState = new GameState { Id = "dummy" };
        GameStateStore.Set("dummy", gameState);
        var sut = new GameAuthorizationService();

        var result = sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenPlayerNotStoryTeller()
    {
        string userId = CommonMethods.GetRandomStringId();
        string gameId = CommonMethods.GetRandomStringId();
        var gameState = CreateGameState(gameId, [new ValueTuple<string, UserType>(userId, UserType.Player)]);

        GameStateStore.Set(gameId, gameState);
        var sut = new GameAuthorizationService();

        var result = sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsTrue_WhenPlayerIsStoryTeller()
    {
        string userId = CommonMethods.GetRandomStringId();
        string gameId = CommonMethods.GetRandomStringId();
        var gameState = CreateGameState(gameId, [new ValueTuple<string, UserType>(userId, UserType.StoryTeller)]);

        GameStateStore.Set(gameId, gameState);
        var sut = new GameAuthorizationService();

        var result = sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeTrue();
    }
}