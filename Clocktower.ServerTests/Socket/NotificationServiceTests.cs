using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.ServerTests.Socket;

[TestClass]
public class NotificationServiceTests
{
    private Mock<IHubContext<DiscordNotificationHub, IDiscordNotificationClient>> _mockHubContext = null!;
    private Mock<IHubCallerClients<IDiscordNotificationClient>> _mockClients = null!;
    private Mock<IDiscordNotificationClient> _mockClientProxy = null!;
    private Mock<IGroupManager> _mockGroups = null!;
    private NotificationService Sut => new(_mockHubContext.Object);

    [TestInitialize]
    public void Setup()
    {
        _mockHubContext = new Mock<IHubContext<DiscordNotificationHub, IDiscordNotificationClient>>();
        _mockClients = new Mock<IHubCallerClients<IDiscordNotificationClient>>();
        _mockClientProxy = new Mock<IDiscordNotificationClient>();
        _mockGroups = new Mock<IGroupManager>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockHubContext.Setup(h => h.Groups).Returns(_mockGroups.Object);
    }

    [TestMethod]
    public async Task BroadcastTownOccupancyUpdate_CallsCorrectGroup()
    {
        const string gameId = "test-game-123";
        var occupants = new TownOccupants([
            new MiniCategory(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), [])
        ]);
        _mockClients.Setup(c => c.Group("game:test-game-123")).Returns(_mockClientProxy.Object);

        await Sut.BroadcastTownOccupancyUpdate(gameId, occupants);

        _mockClients.Verify(c => c.Group("game:test-game-123"), Times.Once);
        _mockClientProxy.Verify(cp => cp.TownOccupancyUpdated(occupants), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastUserVoiceStateChanged_CallsCorrectGroupWithParameters()
    {
        const string gameId = "test-game-456";
        const string userId = "user-123";
        const bool inVoice = true;
        var mutedState = new MutedState(true, false, true, false);

        _mockClients.Setup(c => c.Group("game:test-game-456")).Returns(_mockClientProxy.Object);

        await Sut.BroadcastUserVoiceStateChanged(gameId, userId, inVoice, mutedState);

        _mockClients.Verify(c => c.Group("game:test-game-456"), Times.Once);
        _mockClientProxy.Verify(cp => cp.UserVoiceStateChanged(userId, inVoice, mutedState), Times.Once);
    }

    [TestMethod]
    public async Task PingUser_CallsSpecificUser()
    {
        const string targetUserId = "target-user-789";
        const string message = "Test ping message";
        _mockClients.Setup(c => c.User(targetUserId)).Returns(_mockClientProxy.Object);

        await Sut.PingUser(targetUserId, message);

        _mockClients.Verify(c => c.User(targetUserId), Times.Once);
        _mockClientProxy.Verify(cp => cp.PingUser(message), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastTownTime_WithGameId_CallsCorrectGroup()
    {
        const string gameId = "test-game-789";
        const GameTime gameTime = GameTime.Day;
        _mockClients.Setup(c => c.Group("game:test-game-789")).Returns(_mockClientProxy.Object);

        await Sut.BroadcastTownTime(gameId, gameTime);

        _mockClients.Verify(c => c.Group("game:test-game-789"), Times.Once);
        _mockClientProxy.Verify(cp => cp.TownTimeChanged((int)gameTime), Times.Once);
    }
}