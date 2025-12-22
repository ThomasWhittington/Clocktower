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
        _mockGroups = new Mock<IGroupManager>();

        _sut = new NotificationService(_mockHubContext.Object, _mockGamePerspectiveStore.Object, _mockDiscordTownManager.Object);

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockHubContext.Setup(h => h.Groups).Returns(_mockGroups.Object);
    }

//
//     [TestMethod]
//     public async Task BroadcastDiscordTownUpdate_CallsCorrectGroups()
//     {
//         const string gameId = "test-game-123";
//         const string guildId = "123";
//         var discordTown = new DiscordTown([
//             new MiniCategory(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), []),
//             new MiniCategory(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), [])
//         ]);
//         var playerTown = new DiscordTownDto(gameId, []);
//         _mockClients.Setup(c => c.User("player")).Returns(_mockClientProxy1.Object);
//         _mockClients.Setup(c => c.Users(new[] { "storyteller", "spectator" })).Returns(_mockClientProxy2.Object);
//         _mockDiscordTownManager.Setup(o => o.RedactTownDto(It.IsAny<DiscordTownDto>(), "player")).Returns(playerTown);
//         _mockGamePerspectiveStore.Setup(o => o.Get(gameId)).Returns(CommonMethods.GetGamePerspective(gameId, guildId) with
//         {
//             Users =
//             [
//                 new GameUser("player") { UserType = UserType.Player },
//                 new GameUser("storyteller") { UserType = UserType.StoryTeller },
//                 new GameUser("spectator") { UserType = UserType.Spectator }
//             ]
//         });
//         _mockDiscordTownManager.Setup(o => o.GetDiscordTown(guildId)).Returns(discordTown);
//
//         await _sut.BroadcastDiscordTownUpdate(gameId);
//
//         _mockDiscordTownManager.Verify(o => o.RedactTownDto(It.IsAny<DiscordTownDto>(), "player"), Times.Once);
//         _mockClients.Verify(c => c.User("player"), Times.Once);
//         _mockClients.Verify(c => c.Users(new[] { "storyteller", "spectator" }), Times.Once);
//         _mockClientProxy1.Verify(cp => cp.DiscordTownUpdated(playerTown), Times.Once);
//         _mockClientProxy2.Verify(cp => cp.DiscordTownUpdated(It.Is<DiscordTownDto>(town => town.GameId == gameId && town.ChannelCategories.Count == 2)), Times.Once);
//     }
//
//     [TestMethod]
//     public async Task BroadcastDiscordTownUpdate_ExitsEarly_WhenNoGameFound()
//     {
//         const string gameId = "test-game-123";
//         _mockClients.Setup(c => c.Group("game:test-game-123")).Returns(_mockClientProxy1.Object);
//         _mockGamePerspectiveStore.Setup(o => o.Get(gameId)).Returns((GamePerspective?)null);
//
//         await _sut.BroadcastDiscordTownUpdate(gameId);
//
//         _mockGamePerspectiveStore.Verify(o => o.Get(gameId), Times.Once);
//         _mockDiscordTownManager.Verify(c => c.GetDiscordTown(It.IsAny<string>()), Times.Never);
//     }
//
//     [TestMethod]
//     public async Task BroadcastDiscordTownUpdate_ExitsEarly_WhenNoTownFound()
//     {
//         const string gameId = "test-game-123";
//         const string guildId = "123";
//         _mockClients.Setup(c => c.Group($"game:{gameId}")).Returns(_mockClientProxy1.Object);
//         _mockGamePerspectiveStore.Setup(o => o.Get(gameId)).Returns(CommonMethods.GetGamePerspective(gameId, guildId));
//         _mockDiscordTownManager.Setup(o => o.GetDiscordTown(guildId)).Returns((DiscordTown?)null);
//
//         await _sut.BroadcastDiscordTownUpdate(gameId);
//
//         _mockGamePerspectiveStore.Verify(o => o.Get(gameId), Times.Once);
//         _mockDiscordTownManager.Verify(c => c.GetDiscordTown(guildId), Times.Once);
//         _mockClients.Verify(c => c.Group($"game:{gameId}"), Times.Never);
//     }
//
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