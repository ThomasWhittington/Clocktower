using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.ServerTests.Socket;

[TestClass]
public class DiscordNotificationHubTests
{
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private Mock<IGroupManager> _mockGroups = null!;
    private Mock<HubCallerContext> _mockContext = null!;

    private DiscordNotificationHub Sut
    {
        get
        {
            var discordNotificationHub = new DiscordNotificationHub(_mockGameStateStore.Object, _mockJwtWriter.Object);
            discordNotificationHub.Context = _mockContext.Object;
            discordNotificationHub.Groups = _mockGroups.Object;
            return discordNotificationHub;
        }
    }

    [TestInitialize]
    public void Setup()
    {
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockJwtWriter = new Mock<IJwtWriter>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();


        var hubContextMock = new Mock<IHubContext<DiscordNotificationHub>>();
        hubContextMock.Setup(h => h.Groups).Returns(_mockGroups.Object);
    }

    [TestMethod]
    public async Task JoinGameGroup_ReturnsNull_WhenGameStateNotFound()
    {
        const string gameId = "non-existent-game";
        const string userId = "user-123";
        _mockGameStateStore.Setup(s => s.Get(gameId)).Returns((GameState?)null);

        var result = await Sut.JoinGameGroup(gameId, userId);

        result.Should().BeNull();
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task JoinGameGroup_ReturnsNull_WhenUserNotFoundInGame()
    {
        const string gameId = "test-game";
        const string userId = "non-existent-user";
        var gameState = new GameState { Id = gameId };

        _mockGameStateStore.Setup(s => s.Get(gameId)).Returns(gameState);

        var result = await Sut.JoinGameGroup(gameId, userId);

        result.Should().BeNull();
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task JoinGameGroup_ReturnsSessionState_WhenUserFoundInGame()
    {
        const string gameId = "test-game";
        const string userId = "user-123";
        var gameUser = new GameUser(userId, "TestUser", "avatar-url");
        var gameState = new GameState
        {
            Id = gameId,
            GameTime = GameTime.Day,
            Users = [gameUser]
        };
        const string expectedJwt = "jwt-token-123";

        _mockGameStateStore.Setup(s => s.Get(gameId)).Returns(gameState);
        _mockJwtWriter.Setup(j => j.GetJwtToken(gameUser)).Returns(expectedJwt);
        _mockContext.Setup(c => c.ConnectionId).Returns("connection-456");

        var result = await Sut.JoinGameGroup(gameId, userId);

        result.Should().NotBeNull();
        result!.GameTime.Should().Be(GameTime.Day);
        result.Jwt.Should().Be(expectedJwt);
        _mockGroups.Verify(g => g.AddToGroupAsync("connection-456", "game:test-game"), Times.Once);
    }

    [TestMethod]
    public async Task LeaveGameGroup_CallsGroupManager()
    {
        const string gameId = "test-game";
        _mockContext.Setup(c => c.ConnectionId).Returns("connection-789");

        await Sut.LeaveGameGroup(gameId);

        _mockGroups.Verify(g => g.RemoveFromGroupAsync("connection-789", "game:test-game"), Times.Once);
    }
}