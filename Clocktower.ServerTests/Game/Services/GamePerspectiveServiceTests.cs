using System.IO.Abstractions;
using System.Text.Json;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Game.Services;
using Clocktower.Server.Socket;

namespace Clocktower.ServerTests.Game.Services;

[TestClass]
public class GameStateServiceTests
{
    private const string DummyJsonFile = "dummyState.json";

    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<IGamePerspectiveStore> _mockGameStateStore = null!;
    private Mock<IFileSystem> _mockFileSystem = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockGameStateStore = new Mock<IGamePerspectiveStore>();
        _mockFileSystem = new Mock<IFileSystem>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private IGameStateService Sut => new GameStateService(_mockBot.Object, _mockGameStateStore.Object, _mockFileSystem.Object, _mockNotificationService.Object);

    #region GetGames

    [TestMethod]
    public void GetGames_CallsGameStateStore()
    {
        GamePerspective[] allGames =
        [
            CommonMethods.GetGameState(),
            CommonMethods.GetGameState(),
            CommonMethods.GetGameState()
        ];

        _mockGameStateStore.Setup(o => o.GetAll()).Returns(allGames);

        var result = Sut.GetGames();
        result.Should().BeEquivalentTo(allGames);

        _mockGameStateStore.Verify(o => o.GetAll(), Times.Once);
    }

    #endregion

    #region GetGuildGames

    [TestMethod]
    public void GetGuildGames_CallsGameStateStore()
    {
        var guildId = CommonMethods.GetRandomString();
        GamePerspective[] allGames =
        [
            CommonMethods.GetGameState(),
            CommonMethods.GetGameState(),
            CommonMethods.GetGameState()
        ];

        _mockGameStateStore.Setup(o => o.GetGuildGames(guildId)).Returns(allGames);

        var result = Sut.GetGuildGames(guildId);
        result.Should().BeEquivalentTo(allGames);

        _mockGameStateStore.Verify(o => o.GetGuildGames(guildId), Times.Once);
    }

    #endregion

    #region GetPlayerGames

    [TestMethod]
    public void GetPlayerGames_CallsGameStateStore()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        GamePerspective[] allGames =
        [
            CommonMethods.GetGameState(creatorId: userId),
            CommonMethods.GetGameState(creatorId: userId),
            CommonMethods.GetGameState(creatorId: userId)
        ];
        var expected = allGames.Select(o => new MiniGamePerspective(o.Id, o.CreatedBy, o.CreatedDate));

        _mockGameStateStore.Setup(o => o.GetUserGames(userId)).Returns(allGames);

        var result = Sut.GetPlayerGames(userId);
        result.Should().BeEquivalentTo(expected);

        _mockGameStateStore.Verify(o => o.GetUserGames(userId), Times.Once);
    }

    #endregion

    #region GetGame

    [TestMethod]
    public void GetGame_ReturnsExpected_WhenStoreReturnsGame()
    {
        var gameId = CommonMethods.GetRandomString();
        var gameState = CommonMethods.GetGameState();

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(gameState);

        var result = Sut.GetGame(gameId);

        result.success.Should().BeTrue();
        result.perspectives.Should().Be(gameState);
        result.message.Should().Be("Game retrieved successfully");
    }

    [TestMethod]
    public void GetGame_ReturnsExpected_WhenStoreReturnsNull()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns((GamePerspective)null!);

        var result = Sut.GetGame(gameId);

        result.success.Should().BeFalse();
        result.perspectives.Should().BeNull();
        result.message.Should().Be($"Game ID '{gameId}' not found");
    }

    #endregion

    #region DeleteGame

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsTrue()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGameStateStore.Setup(o => o.RemoveGame(gameId)).Returns(true);

        var result = Sut.DeleteGame(gameId);

        result.success.Should().BeTrue();
        result.message.Should().Be($"Game deleted successfully");
    }

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGameStateStore.Setup(o => o.RemoveGame(gameId)).Returns(false);

        var result = Sut.DeleteGame(gameId);

        result.success.Should().BeFalse();
        result.message.Should().Be($"Game ID '{gameId}' failed to be deleted");
    }

    #endregion

    #region StartNewGame

    [TestMethod]
    public void StartNewGame_ReturnsFalse_WhenNoUserFound()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();

        _mockBot.Setup(o => o.GetUser(userId)).Returns((IDiscordUser)null!);

        var result = Sut.StartNewGame(guildId, gameId, userId);

        result.success.Should().BeFalse();
        result.gameState.Should().BeNull();
        result.message.Should().Be("Couldn't find user");
    }

    [TestMethod]
    public void StartNewGame_ReturnsExpected_WhenGameStateStoreSetReturnsTrue()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var userName = CommonMethods.GetRandomString();
        var userAvatarUrl = CommonMethods.GetRandomString();
        var expectedGameUser = new GameUser(userId)
        {
            UserType = UserType.StoryTeller
        };
        var expectedGameState = CommonMethods.GetGameState(gameId, guildId, createdBy: expectedGameUser) with { Users = [expectedGameUser] };

        var mockedUser = MockMaker.CreateMockDiscordUser(userId, userName, userAvatarUrl);
        _mockBot.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGameStateStore.Setup(o => o.Set(It.Is<GamePerspective>(g => g.Id == gameId))).Returns(true);

        var result = Sut.StartNewGame(guildId, gameId, userId);

        result.success.Should().BeTrue();
        result.gameState.Should().BeEquivalentTo(expectedGameState, options => options
            .Excluding(x => x.CreatedDate));
        result.gameState.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.message.Should().Be("Game started successfully");
    }

    [TestMethod]
    public void StartNewGame_ReturnsExpected_WhenGameStateStoreSetReturnsFalse()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var userName = CommonMethods.GetRandomString();
        var userAvatarUrl = CommonMethods.GetRandomString();

        var mockedUser = MockMaker.CreateMockDiscordUser(userId, userName, userAvatarUrl);
        _mockBot.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGameStateStore.Setup(o => o.Set(It.Is<GamePerspective>(g => g.Id == gameId))).Returns(false);

        var result = Sut.StartNewGame(guildId, gameId, userId);

        result.success.Should().BeFalse();
        result.gameState.Should().BeNull();
        result.message.Should().Be($"Game Id '{gameId}' already exists");
    }

    #endregion
/*
    #region LoadDummyData

    [TestMethod]
    public void LoadDummyData_ReturnsSuccess_WhenValidJsonFileExists()
    {
        var validJson = JsonSerializer.Serialize(new[]
        {
            CommonMethods.GetGameState("game1", "guild1"),
            CommonMethods.GetGameState("game2", "guild2")
        });

        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(validJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        result.message.Should().Be("Loaded dummy data");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Once);
        _mockGameStateStore.Verify(s => s.Set(It.Is<GameState>(o => o.Id == "game1")), Times.Once);
        _mockGameStateStore.Verify(s => s.Set(It.Is<GameState>(o => o.Id == "game2")), Times.Once);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenJsonDeserializationFails()
    {
        const string invalidJson = "{ invalid json }";
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(invalidJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to deserialize json");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
        _mockGameStateStore.Verify(s => s.Set(It.IsAny<GameState>()), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenJsonDeserializesToNull()
    {
        const string nullJson = "null";
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(nullJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to deserialize json");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ClearsStoreBeforeLoading()
    {
        var validJson = JsonSerializer.Serialize(new[] { CommonMethods.GetGameState() });
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(validJson);

        Sut.LoadDummyData();

        var invocations = _mockGameStateStore.Invocations.ToList();
        invocations.Should().HaveCountGreaterThan(0);
        invocations[0].Method.Name.Should().Be("Clear");
    }

    [TestMethod]
    public void LoadDummyData_LoadsEmptyArray_Successfully()
    {
        const string emptyArrayJson = "[]";
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(emptyArrayJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        result.message.Should().Be("Loaded dummy data");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Once);
        _mockGameStateStore.Verify(s => s.Set(It.IsAny<GameState>()), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenFileDoesNotExist()
    {
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile))
            .Throws<FileNotFoundException>();

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("File not found");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenUnknownException()
    {
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile))
            .Throws(new Exception("Custom error message"));

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be($"Error loading dummy data: Custom error message");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_LoadsMultipleGames_InCorrectOrder()
    {
        var games = new[]
        {
            CommonMethods.GetGameState("first"),
            CommonMethods.GetGameState("second"),
            CommonMethods.GetGameState("third")
        };
        var validJson = JsonSerializer.Serialize(games);
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(validJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        _mockGameStateStore.Verify(s => s.Set(It.Is<GameState>(g => g.Id == "first")), Times.Once);
        _mockGameStateStore.Verify(s => s.Set(It.Is<GameState>(g => g.Id == "second")), Times.Once);
        _mockGameStateStore.Verify(s => s.Set(It.Is<GameState>(g => g.Id == "third")), Times.Once);
    }

    #endregion
*/

    #region SetTime

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenGameNotFound()
    {
        const string gameId = "game-id";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns((GamePerspective?)null);

        var result = await Sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be("Game not found");
    }

    [TestMethod]
    [DynamicData(nameof(GetGameTimeValues))]
    public async Task SetTime_SetsTime_NotifyClients_WhenDataGood(GameTime gameTime)
    {
        const string gameId = "game-id";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(CommonMethods.GetGameState(gameId));

        var result = await Sut.SetTime(gameId, gameTime);

        _mockGameStateStore.Verify(o => o.SetTime(gameId, gameTime), Times.Once);
        _mockNotificationService.Verify(o => o.BroadcastTownTime(gameId, gameTime), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be($"Time set to {gameTime}");
    }

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenExceptionThrown()
    {
        const string gameId = "game-id";
        const string message = "message";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Throws(new Exception(message));

        var result = await Sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be(message);
    }

    #endregion

    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
}