using Clocktower.Server.Data;
using Clocktower.Server.Data.Dto;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.ServerTests.Socket;

[TestClass]
public class NotificationServiceTests
{
    private Mock<IHubContext<DiscordNotificationHub, IDiscordNotificationClient>> _mockHubContext = null!;
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
        _mockClients = new Mock<IHubCallerClients<IDiscordNotificationClient>>();
        _mockClientProxy1 = new Mock<IDiscordNotificationClient>();
        _mockClientProxy2 = new Mock<IDiscordNotificationClient>();
        _mockClientProxy3 = new Mock<IDiscordNotificationClient>();
        _mockGroups = new Mock<IGroupManager>();

        _sut = new NotificationService(_mockHubContext.Object);

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockHubContext.Setup(h => h.Groups).Returns(_mockGroups.Object);
    }

    [TestMethod]
    public async Task SendBulkDiscordTownUpdates_CallsCorrectGroups()
    {
        const string gameId = "test-game-123";
        var discordTownDto = new DiscordTownDto(gameId, [
            new MiniCategoryDto(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), []),
            new MiniCategoryDto(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), []),
        ]);
        var notifications = new List<UserNotification>
        {
            new("player", discordTownDto),
            new("storyteller", discordTownDto),
            new("spectator", discordTownDto),
        };

        _mockClients.Setup(c => c.User("player")).Returns(_mockClientProxy1.Object);
        _mockClients.Setup(c => c.User("storyteller")).Returns(_mockClientProxy2.Object);
        _mockClients.Setup(c => c.User("spectator")).Returns(_mockClientProxy3.Object);

        await _sut.SendBulkDiscordTownUpdates(notifications);

        _mockClients.Verify(c => c.User("player"), Times.Once);
        _mockClients.Verify(c => c.User("storyteller"), Times.Once);
        _mockClients.Verify(c => c.User("spectator"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.DiscordTownUpdated(discordTownDto), Times.Once);
        _mockClientProxy2.Verify(cp => cp.DiscordTownUpdated(discordTownDto), Times.Once);
        _mockClientProxy3.Verify(cp => cp.DiscordTownUpdated(discordTownDto), Times.Once);
    }

    [TestMethod]
    public async Task SendTimerUpdateToGroup_CallsCorrectGroup()
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

        await _sut.SendTimerUpdateToGroup(gameId, timer);

        _mockClients.Verify(c => c.Group("game:test-game-123"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.TimerUpdated(timer), Times.Once);
    }

    [TestMethod]
    public async Task SendUserVoiceStateToGroup_CallsCorrectGroup()
    {
        const string gameId = "test-game-456";
        const string userId = "user-123";
        const bool inVoice = true;
        var voiceState = new VoiceState(true, false, true, false);

        _mockClients.Setup(c => c.Group("game:test-game-456")).Returns(_mockClientProxy1.Object);

        await _sut.SendUserVoiceStateToGroup(gameId, userId, inVoice, voiceState);

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
    public async Task SendTownTimeToGroup_WithGameId_CallsCorrectGroup()
    {
        const string gameId = "test-game-789";
        const GameTime gameTime = GameTime.Day;
        _mockClients.Setup(c => c.Group("game:test-game-789")).Returns(_mockClientProxy1.Object);

        await _sut.SendTownTimeToGroup(gameId, gameTime);

        _mockClients.Verify(c => c.Group("game:test-game-789"), Times.Once);
        _mockClientProxy1.Verify(cp => cp.TownTimeChanged(gameId, (int)gameTime), Times.Once);
    }
}