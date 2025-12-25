using Clocktower.Server.Data;
using Clocktower.Server.Data.Dto;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Clocktower.Server.Socket.Services;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.ServerTests.Socket;

[TestClass]
public class DiscordNotificationHubTests
{
    private const string GameId = "game-id";
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
        var discordTown = new DiscordTownDto(GameId, [new MiniCategoryDto(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        var result = new SessionSyncState
        {
            GameTime = GameTime.Day,
            Jwt = jwt,
            DiscordTown = discordTown,
            Timer = new TimerState
            {
                GameId = GameId,
                Status = TimerStatus.Running,
                ServerNowUtc = DateTime.UtcNow,
                EndUtc = DateTime.UtcNow.AddSeconds(30)
            }
        };
        return result;
    }

    [TestMethod]
    public async Task JoinGameGroup_ReturnsSessionState_WhenUserFoundInGameWithoutLeaveGame()
    {
        const string gameId = "test-game";
        const string oldGameId = "";
        const string userId = "user-123";
        const string jwt = "jwt-token-123";
        const string connection = "connection-456";
        var sessionSyncState = GetSessionSyncState(jwt);
        _mockContext.Setup(c => c.ConnectionId).Returns(connection);
        _mockHubStateManager.Setup(o => o.GetState(gameId, userId)).Returns(sessionSyncState);

        var result = await _sut.JoinGameGroup(gameId, userId, oldGameId);

        result.Should().Be(sessionSyncState);
        _mockHubStateManager.Verify(o => o.GetState(gameId, userId), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync(connection, $"game:{gameId}"), Times.Once);
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task JoinGameGroup_ReturnsSessionState_WhenUserFoundInGame()
    {
        const string gameId = "test-game";
        const string oldGameId = "old-game-id";
        const string userId = "user-123";
        const string jwt = "jwt-token-123";
        const string connection = "connection-456";
        var sessionSyncState = GetSessionSyncState(jwt);
        _mockContext.Setup(c => c.ConnectionId).Returns(connection);
        _mockHubStateManager.Setup(o => o.GetState(gameId, userId)).Returns(sessionSyncState);

        var result = await _sut.JoinGameGroup(gameId, userId, oldGameId);

        result.Should().Be(sessionSyncState);
        _mockHubStateManager.Verify(o => o.GetState(gameId, userId), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync(connection, $"game:{gameId}"), Times.Once);
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(connection, $"game:{oldGameId}"), Times.Once);
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