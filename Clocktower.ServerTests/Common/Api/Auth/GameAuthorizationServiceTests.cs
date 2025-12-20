using Clocktower.Server.Common.Api.Auth;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Common.Api.Auth;

[TestClass]
public class GameAuthorizationServiceTests
{
    private Mock<IGamePerspectiveStore> _mockGamePerspectiveStore = null!;
    private IGameAuthorizationService Sut => new GameAuthorizationService(_mockGamePerspectiveStore.Object);


    private void MockResponse(string gameId, string userId, GamePerspective gamePerspective)
    {
        _mockGamePerspectiveStore.Setup(o =>
                o.Get(gameId, userId))
            .Returns(gamePerspective);
    }


    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveStore = new Mock<IGamePerspectiveStore>();
    }


    private static GamePerspective CreateGamePerspective(string gameId, List<(string userId, UserType userType)>? users = null)
    {
        var gameUsers = new List<GameUser>();
        if (users is not null)
        {
            foreach ((string userId, UserType userType) in users)
            {
                gameUsers.Add(new GameUser(userId)
                {
                    UserType = userType
                });
            }
        }

        return CommonMethods.GetGamePerspective(gameId) with { Users = gameUsers };
    }


    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenUnknownGameId()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gamePerspective = CreateGamePerspective("dummy", [new ValueTuple<string, UserType>(userId, UserType.StoryTeller)]);
        MockResponse("dummy", userId, gamePerspective);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnFalse_WhenUnknownUserId()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gamePerspective = CommonMethods.GetGamePerspective(gameId);
        MockResponse(gameId, userId, gamePerspective);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenUnknownUserIdUnknownGameId()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gamePerspective = CommonMethods.GetGamePerspective("dummy");
        MockResponse(gameId, userId, gamePerspective);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsFalse_WhenPlayerNotStoryTeller()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gamePerspective = CreateGamePerspective(gameId, [new ValueTuple<string, UserType>(userId, UserType.Player)]);
        MockResponse(gameId, userId, gamePerspective);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void IsStoryTellerForGame_ReturnsTrue_WhenPlayerIsStoryTeller()
    {
        string userId = CommonMethods.GetRandomString();
        string gameId = CommonMethods.GetRandomString();
        var gamePerspective = CreateGamePerspective(gameId, [new ValueTuple<string, UserType>(userId, UserType.StoryTeller)]);
        MockResponse(gameId, userId, gamePerspective);

        var result = Sut.IsStoryTellerForGame(userId, gameId);

        result.Should().BeTrue();
    }
}