using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket.Services;

namespace Clocktower.ServerTests.Socket.Services;

[TestClass]
public class HubStateManagerTests
{
    private Mock<IGamePerspectiveStore> _mockGamePerspectiveStore = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private Mock<ITimerCoordinator> _mockTimerCoordinator = null!;
    private IHubStateManager _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockGamePerspectiveStore = StrictMockFactory.Create<IGamePerspectiveStore>();
        _mockDiscordTownManager = StrictMockFactory.Create<IDiscordTownManager>();
        _mockJwtWriter = StrictMockFactory.Create<IJwtWriter>();
        _mockTimerCoordinator = StrictMockFactory.Create<ITimerCoordinator>();

        _sut = new HubStateManager(_mockGamePerspectiveStore.Object,
            _mockDiscordTownManager.Object,
            _mockJwtWriter.Object,
            _mockTimerCoordinator.Object
        );
    }


    [TestMethod]
    public void GetState_ReturnsNull_WhenGamePerspectiveNotFound()
    {
        const string gameId = "non-existent-game";
        const string userId = "user-123";
        _mockGamePerspectiveStore.Setup(s => s.Get(gameId, userId)).Returns((GamePerspective?)null);

        var result = _sut.GetState(gameId, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetState_ReturnsNull_WhenUserNotFoundInGame()
    {
        const string gameId = "test-game";
        const string userId = "non-existent-user";
        var gamePerspective = CommonMethods.GetGamePerspective(gameId);

        _mockGamePerspectiveStore.Setup(s => s.Get(gameId, userId)).Returns(gamePerspective);

        var result = _sut.GetState(gameId, userId);

        result.Should().BeNull();
    }


    [TestMethod]
    public void GetState_ReturnsSessionState_WhenUserFoundInGame_Player()
    {
        const string gameId = "test-game";
        const string guildId = "1";
        const string userId = "user-123";
        var gameUser = new GameUser(userId) { UserType = UserType.Player };
        var gamePerspective = CommonMethods.GetGamePerspective(gameId, guildId) with { GameTime = GameTime.Day, Users = [gameUser] };

        var timer = new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.Running,
            ServerNowUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddSeconds(30)
        };
        var discordTown = new DiscordTown([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        const string expectedJwt = "jwt-token-123";
        _mockGamePerspectiveStore.Setup(s => s.Get(gameId, userId)).Returns(gamePerspective);
        _mockDiscordTownManager.Setup(s => s.GetDiscordTown(guildId)).Returns(discordTown);
        _mockDiscordTownManager.Setup(o => o.RedactTownDto(It.IsAny<DiscordTownDto>(), userId)).Returns(new DiscordTownDto(gameId, []));
        _mockJwtWriter.Setup(j => j.GetJwtToken(gameUser)).Returns(expectedJwt);
        _mockTimerCoordinator.Setup(t => t.Get(gameId)).Returns(timer);

        var result = _sut.GetState(gameId, userId);

        _mockGamePerspectiveStore.Verify(o => o.Get(gameId, userId), Times.Once);
        _mockDiscordTownManager.Verify(o => o.GetDiscordTown(guildId), Times.Once);
        _mockDiscordTownManager.Verify(o => o.RedactTownDto(It.IsAny<DiscordTownDto>(), userId), Times.Once);
        _mockJwtWriter.Verify(o => o.GetJwtToken(gameUser), Times.Once);
        result.Should().NotBeNull();
        result!.GameTime.Should().Be(GameTime.Day);
        result.Jwt.Should().Be(expectedJwt);
        result.DiscordTown.Should().NotBeNull();
        result.DiscordTown.GameId.Should().Be(gameId);
        result.Timer.Should().Be(timer);
    }


    [TestMethod]
    public void GetState_ReturnsSessionState_WhenUserFoundInGame_Storyteller()
    {
        const string gameId = "test-game";
        const string guildId = "1";
        const string userId = "user-123";
        var gameUser = new GameUser(userId) { UserType = UserType.StoryTeller };
        var gamePerspective = CommonMethods.GetGamePerspective(gameId, guildId) with { GameTime = GameTime.Day, Users = [gameUser] };

        var timer = new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.Running,
            ServerNowUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddSeconds(30)
        };
        var discordTown = new DiscordTown([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        const string expectedJwt = "jwt-token-123";
        _mockGamePerspectiveStore.Setup(s => s.Get(gameId, userId)).Returns(gamePerspective);
        _mockDiscordTownManager.Setup(s => s.GetDiscordTown(guildId)).Returns(discordTown);
        _mockJwtWriter.Setup(j => j.GetJwtToken(gameUser)).Returns(expectedJwt);
        _mockTimerCoordinator.Setup(t => t.Get(gameId)).Returns(timer);

        var result = _sut.GetState(gameId, userId);

        _mockGamePerspectiveStore.Verify(o => o.Get(gameId, userId), Times.Once);
        _mockDiscordTownManager.Verify(o => o.GetDiscordTown(guildId), Times.Once);
        _mockJwtWriter.Verify(o => o.GetJwtToken(gameUser), Times.Once);
        result.Should().NotBeNull();
        result!.GameTime.Should().Be(GameTime.Day);
        result.Jwt.Should().Be(expectedJwt);
        result.DiscordTown.Should().NotBeNull();
        result.DiscordTown.GameId.Should().Be(gameId);
        result.Timer.Should().Be(timer);
    }
}