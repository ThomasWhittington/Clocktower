using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Clocktower.Server.Socket.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.ServerTests.Socket;

[TestClass]
public class DiscordNotificationHubTests
{
    private Mock<IGroupManager> _mockGroups = null!;
    private Mock<HubCallerContext> _mockContext = null!;
    private Mock<IHubStateManager> _mockHubStateManager = null!;

    private DiscordNotificationHub _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();
        _mockHubStateManager = new Mock<IHubStateManager>();
        _sut = new DiscordNotificationHub(_mockHubStateManager.Object);
        _sut.Context = _mockContext.Object;
        _sut.Groups = _mockGroups.Object;

        var hubContextMock = new Mock<IHubContext<DiscordNotificationHub>>();
        hubContextMock.Setup(h => h.Groups).Returns(_mockGroups.Object);
    }

    private static SessionSyncState GetSessionSyncState(string jwt)
    {
        var townOccupancy = new TownOccupants([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        var result = new SessionSyncState
        {
            GameTime = GameTime.Day,
            Jwt = jwt,
            TownOccupancy = townOccupancy
        };
        return result;
    }

    [TestMethod]
    public async Task JoinGameGroup_ReturnsSessionState_WhenUserFoundInGame()
    {
        const string gameId = "test-game";
        const string userId = "user-123";
        const string jwt = "jwt-token-123";
        var sessionSyncState = GetSessionSyncState(jwt);
        _mockContext.Setup(c => c.ConnectionId).Returns("connection-456");
        _mockHubStateManager.Setup(o => o.GetState(gameId, userId)).Returns(sessionSyncState);

        var result = await _sut.JoinGameGroup(gameId, userId);

        result.Should().Be(sessionSyncState);
        _mockHubStateManager.Verify(o => o.GetState(gameId, userId), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync("connection-456", "game:test-game"), Times.Once);
    }

    [TestMethod]
    public async Task LeaveGameGroup_CallsGroupManager()
    {
        const string gameId = "test-game";
        _mockContext.Setup(c => c.ConnectionId).Returns("connection-789");

        await _sut.LeaveGameGroup(gameId);

        _mockGroups.Verify(g => g.RemoveFromGroupAsync("connection-789", "game:test-game"), Times.Once);
    }
}