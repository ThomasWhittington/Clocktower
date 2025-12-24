using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.ServerTests.Socket;

[TestClass]
public class NotificationServiceTests
{
    private Mock<IHubContext<DiscordNotificationHub, IDiscordNotificationClient>> _mockHubContext = null!;
    private Mock<IGamePerspectiveStore> _mockGamePerspectiveStore = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<IHubCallerClients<IDiscordNotificationClient>> _mockClients = null!;
    private Mock<IDiscordNotificationClient> _mockClientProxy1 = null!;
    private Mock<IDiscordNotificationClient> _mockClientProxy2 = null!;
    private Mock<IDiscordNotificationClient> _mockClientProxy3 = null!;
    private Mock<IGroupManager> _mockGroups = null!;
    private INotificationService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHubContext = new Mock<IHubContext<DiscordNotificationHub, IDiscordNotificationClient>>();
        _mockGamePerspectiveStore = new Mock<IGamePerspectiveStore>();
        _mockDiscordTownManager = StrictMockFactory.Create<IDiscordTownManager>();
        _mockClients = new Mock<IHubCallerClients<IDiscordNotificationClient>>();
        _mockClientProxy1 = new Mock<IDiscordNotificationClient>();
        _mockClientProxy2 = new Mock<IDiscordNotificationClient>();
        _mockClientProxy3 = new Mock<IDiscordNotificationClient>();
        _mockGroups = new Mock<IGroupManager>();

        _sut = new NotificationService(_mockHubContext.Object, _mockGamePerspectiveStore.Object, _mockDiscordTownManager.Object);

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockHubContext.Setup(h => h.Groups).Returns(_mockGroups.Object);
    }


    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_ExitsEarly_WhenNoGameFound()
    {
        const string gameId = "test-game-123";
        _mockClients.Setup(c => c.Group("game:test-game-123")).Returns(_mockClientProxy1.Object);
        _mockGamePerspectiveStore.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns([]);

        await _sut.BroadcastDiscordTownUpdate(gameId);

        _mockGamePerspectiveStore.Verify(o => o.GetAllPerspectivesForGame(gameId), Times.Once);
        _mockDiscordTownManager.Verify(c => c.GetDiscordTown(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_ExitsEarly_WhenNoTownFound()
    {
        const string gameId = "test-game-123";
        const string guildId = "123";
        _mockClients.Setup(c => c.Group($"game:{gameId}")).Returns(_mockClientProxy1.Object);
        _mockGamePerspectiveStore.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns([CommonMethods.GetGamePerspective(gameId, guildId: guildId)]);
        _mockDiscordTownManager.Setup(o => o.GetDiscordTownDto(guildId, gameId, new List<GameUser>())).Returns((DiscordTownDto?)null);

        await _sut.BroadcastDiscordTownUpdate(gameId);

        _mockGamePerspectiveStore.Verify(o => o.GetAllPerspectivesForGame(gameId), Times.Once);
        _mockDiscordTownManager.Verify(c => c.GetDiscordTownDto(guildId, gameId, new List<GameUser>()), Times.Once);
        _mockClients.Verify(c => c.Group($"game:{gameId}"), Times.Never);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_CallsCorrectGroups()
    {
        const string gameId = "test-game-123";
        const string guildId = "123";
        var discordTownDto = new DiscordTownDto(gameId, [
            new MiniCategoryDto(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), []),
            new MiniCategoryDto(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), []),
        ]);

        var users = new GameUser[]
        {
            new("player") { UserType = UserType.Player },
            new("storyteller") { UserType = UserType.StoryTeller },
            new("spectator") { UserType = UserType.Spectator }
        };

        var playerTown = new DiscordTownDto(gameId, []);
        _mockClients.Setup(c => c.User("player")).Returns(_mockClientProxy1.Object);
        _mockClients.Setup(c => c.User("storyteller")).Returns(_mockClientProxy2.Object);
        _mockClients.Setup(c => c.User("spectator")).Returns(_mockClientProxy3.Object);
        _mockDiscordTownManager.Setup(o => o.RedactTownDto(It.IsAny<DiscordTownDto>(), "player")).Returns(playerTown);
        _mockGamePerspectiveStore.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns(
        [
            CommonMethods.GetGamePerspective(gameId, "player", guildId) with { Users = users },
            CommonMethods.GetGamePerspective(gameId, "storyteller", guildId) with { Users = users },
            CommonMethods.GetGamePerspective(gameId, "spectator", guildId) with { Users = users }
        ]);
        _mockDiscordTownManager.Setup(o => o.GetDiscordTownDto(guildId, gameId, users)).Returns(discordTownDto);

        await _sut.BroadcastDiscordTownUpdate(gameId);

        _mockDiscordTownManager.Verify(o => o.RedactTownDto(It.IsAny<DiscordTownDto>(), "player"), Times.Once);
        _mockClients.Verify(c => c.User("player"), Times.Once);
        _mockClients.Verify(c => c.User("storyteller"), Times.Once);
        _mockClients.Verify(c => c.User("spectator"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.DiscordTownUpdated(playerTown), Times.Once);
        _mockClientProxy2.Verify(cp => cp.DiscordTownUpdated(It.Is<DiscordTownDto>(town => town.GameId == gameId && town.ChannelCategories.Count == 2)), Times.Once);
        _mockClientProxy3.Verify(cp => cp.DiscordTownUpdated(It.Is<DiscordTownDto>(town => town.GameId == gameId && town.ChannelCategories.Count == 2)), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastTimerUpdate_CallsCorrectGroup()
    {
        const string gameId = "test-game-123";
        var timer = new TimerState
        {
            GameId = gameId,
            Status = TimerStatus.Running,
            ServerNowUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddSeconds(30),
            Label = "label"
        };

        _mockClients.Setup(c => c.Group("game:test-game-123")).Returns(_mockClientProxy1.Object);

        await _sut.BroadcastTimerUpdate(gameId, timer);

        _mockClients.Verify(c => c.Group("game:test-game-123"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.TimerUpdated(timer), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastUserVoiceStateChanged_CallsCorrectGroupWithParameters()
    {
        const string gameId = "test-game-456";
        const string userId = "user-123";
        const bool inVoice = true;
        var voiceState = new VoiceState(true, false, true, false);

        _mockClients.Setup(c => c.Group("game:test-game-456")).Returns(_mockClientProxy1.Object);

        await _sut.BroadcastUserVoiceStateChanged(gameId, userId, inVoice, voiceState);

        _mockClients.Verify(c => c.Group("game:test-game-456"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.UserVoiceStateChanged(userId, inVoice, voiceState), Times.Once);
    }

    [TestMethod]
    public async Task PingUser_CallsSpecificUser()
    {
        const string targetUserId = "target-user-789";
        const string message = "Test ping message";
        _mockClients.Setup(c => c.User(targetUserId)).Returns(_mockClientProxy1.Object);

        await _sut.PingUser(targetUserId, message);

        _mockClients.Verify(c => c.User(targetUserId), Times.Once);
        _mockClientProxy1.Verify(cp => cp.PingUser(message), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastTownTime_WithGameId_CallsCorrectGroup()
    {
        const string gameId = "test-game-789";
        const GameTime gameTime = GameTime.Day;
        _mockClients.Setup(c => c.Group("game:test-game-789")).Returns(_mockClientProxy1.Object);

        await _sut.BroadcastTownTime(gameId, gameTime);

        _mockClients.Verify(c => c.Group("game:test-game-789"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.TownTimeChanged((int)gameTime), Times.Once);
    }
}