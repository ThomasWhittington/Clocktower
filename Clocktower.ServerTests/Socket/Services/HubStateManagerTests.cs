using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket.Services;

namespace Clocktower.ServerTests.Socket.Services;

[TestClass]
public class HubStateManagerTests
{
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<ITownOccupancyStore> _mockTownOccupancyStore = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private IHubStateManager _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockTownOccupancyStore = new Mock<ITownOccupancyStore>();
        _mockJwtWriter = new Mock<IJwtWriter>();
        _sut = new HubStateManager(_mockGameStateStore.Object, _mockTownOccupancyStore.Object, _mockJwtWriter.Object);
    }

    [TestMethod]
    public void GetState_ReturnsNull_WhenGameStateNotFound()
    {
        const string gameId = "non-existent-game";
        const string userId = "user-123";
        _mockGameStateStore.Setup(s => s.Get(gameId)).Returns((GameState?)null);

        var result = _sut.GetState(gameId, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetState_ReturnsNull_WhenUserNotFoundInGame()
    {
        const string gameId = "test-game";
        const string userId = "non-existent-user";
        var gameState = new GameState { Id = gameId };

        _mockGameStateStore.Setup(s => s.Get(gameId)).Returns(gameState);

        var result = _sut.GetState(gameId, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetState_ReturnsSessionState_WhenUserFoundInGame()
    {
        const string gameId = "test-game";
        const string guildId = "1";
        const string userId = "user-123";
        var gameUser = new GameUser(userId);
        var gameState = new GameState
        {
            Id = gameId,
            GameTime = GameTime.Day,
            Users = [gameUser],
            GuildId = guildId
        };
        var townOccupancy = new TownOccupants([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        const string expectedJwt = "jwt-token-123";
        _mockGameStateStore.Setup(s => s.Get(gameId)).Returns(gameState);
        _mockTownOccupancyStore.Setup(s => s.Get(guildId)).Returns(townOccupancy);
        _mockJwtWriter.Setup(j => j.GetJwtToken(gameUser)).Returns(expectedJwt);

        var result = _sut.GetState(gameId, userId);

        _mockGameStateStore.Verify(o => o.Get(gameId), Times.Once);
        _mockTownOccupancyStore.Verify(o => o.Get(guildId), Times.Once);
        _mockJwtWriter.Verify(o => o.GetJwtToken(gameUser), Times.Once);
        result.Should().NotBeNull();
        result!.GameTime.Should().Be(GameTime.Day);
        result.Jwt.Should().Be(expectedJwt);
        result.TownOccupancy.Should().Be(townOccupancy);
    }
}