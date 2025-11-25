using System.IO.Abstractions;
using System.Text.Json;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Game.Services;

namespace Clocktower.ServerTests.Game.Services;

[TestClass]
public class GameStateServiceTests
{
    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<IFileSystem> _mockFileSystem = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockFileSystem = new Mock<IFileSystem>();
    }

    private IGameStateService Sut => new GameStateService(_mockBot.Object, _mockGameStateStore.Object, _mockFileSystem.Object);

    #region GetGames

    [TestMethod]
    public void GetGames_CallsGameStateStore()
    {
        GameState[] allGames =
        [
            new() { Id = CommonMethods.GetRandomString() },
            new() { Id = CommonMethods.GetRandomString() },
            new() { Id = CommonMethods.GetRandomString() }
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
        GameState[] allGames =
        [
            new() { Id = CommonMethods.GetRandomString() },
            new() { Id = CommonMethods.GetRandomString() },
            new() { Id = CommonMethods.GetRandomString() }
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
        var user = new GameUser(userId, CommonMethods.GetRandomString(), CommonMethods.GetRandomString());
        GameState[] allGames =
        [
            new() { Id = CommonMethods.GetRandomString(), CreatedDate = DateTime.UtcNow, CreatedBy = user },
            new() { Id = CommonMethods.GetRandomString(), CreatedDate = DateTime.UtcNow, CreatedBy = user },
            new() { Id = CommonMethods.GetRandomString(), CreatedDate = DateTime.UtcNow, CreatedBy = user }
        ];
        var expected = allGames.Select(o => new MiniGameState(o.Id, o.CreatedBy, o.CreatedDate));

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
        var gameState = new GameState { Id = CommonMethods.GetRandomString() };

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(gameState);

        var result = Sut.GetGame(gameId);

        result.success.Should().BeTrue();
        result.gameState.Should().Be(gameState);
        result.message.Should().Be("Game retrieved successfully");
    }

    [TestMethod]
    public void GetGame_ReturnsExpected_WhenStoreReturnsNull()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns((GameState)null!);

        var result = Sut.GetGame(gameId);

        result.success.Should().BeFalse();
        result.gameState.Should().BeNull();
        result.message.Should().Be($"Game ID '{gameId}' not found");
    }

    #endregion

    #region DeleteGame

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsTrue()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGameStateStore.Setup(o => o.Remove(gameId)).Returns(true);

        var result = Sut.DeleteGame(gameId);

        result.success.Should().BeTrue();
        result.message.Should().Be($"Game deleted successfully");
    }

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGameStateStore.Setup(o => o.Remove(gameId)).Returns(false);

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
        var userId = CommonMethods.GetRandomSnowflakeNumberId();

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
        var userId = CommonMethods.GetRandomSnowflakeNumberId();
        var userName = CommonMethods.GetRandomString();
        var userAvatarUrl = CommonMethods.GetRandomString();

        var expectedGameUser = new GameUser(userId.ToString(), userName, userAvatarUrl)
        {
            UserType = UserType.StoryTeller
        };
        var expectedGameState = new GameState
        {
            Id = gameId,
            GuildId = guildId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = expectedGameUser,
            Users = [expectedGameUser]
        };

        var mockedUser = MockMaker.CreateMockDiscordUser(userId, userName, userAvatarUrl);
        _mockBot.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGameStateStore.Setup(o => o.Set(gameId, It.IsAny<GameState>())).Returns(true);

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
        var userId = CommonMethods.GetRandomSnowflakeNumberId();
        var userName = CommonMethods.GetRandomString();
        var userAvatarUrl = CommonMethods.GetRandomString();

        var mockedUser = MockMaker.CreateMockDiscordUser(userId, userName, userAvatarUrl);
        _mockBot.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGameStateStore.Setup(o => o.Set(gameId, It.IsAny<GameState>())).Returns(false);

        var result = Sut.StartNewGame(guildId, gameId, userId);

        result.success.Should().BeFalse();
        result.gameState.Should().BeNull();
        result.message.Should().Be($"Game Id '{gameId}' already exists");
    }

    #endregion

    #region LoadDummyData

    [TestMethod]
    public void LoadDummyData_ReturnsSuccess_WhenValidJsonFileExists()
    {
        var validJson = JsonSerializer.Serialize(new[]
        {
            new GameState { Id = "game1", GuildId = "guild1" },
            new GameState { Id = "game2", GuildId = "guild2" }
        });

        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json")).Returns(validJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        result.message.Should().Be("Loaded dummy data");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Once);
        _mockGameStateStore.Verify(s => s.Set("game1", It.IsAny<GameState>()), Times.Once);
        _mockGameStateStore.Verify(s => s.Set("game2", It.IsAny<GameState>()), Times.Once);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenJsonDeserializationFails()
    {
        const string invalidJson = "{ invalid json }";
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json")).Returns(invalidJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to deserialize json");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
        _mockGameStateStore.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<GameState>()), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenJsonDeserializesToNull()
    {
        const string nullJson = "null";
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json")).Returns(nullJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to deserialize json");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ClearsStoreBeforeLoading()
    {
        var validJson = JsonSerializer.Serialize(new[] { new GameState { Id = "game1" } });
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json")).Returns(validJson);

        Sut.LoadDummyData();

        var invocations = _mockGameStateStore.Invocations.ToList();
        invocations.Should().HaveCountGreaterThan(0);
        invocations[0].Method.Name.Should().Be("Clear");
    }

    [TestMethod]
    public void LoadDummyData_LoadsEmptyArray_Successfully()
    {
        const string emptyArrayJson = "[]";
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json")).Returns(emptyArrayJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        result.message.Should().Be("Loaded dummy data");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Once);
        _mockGameStateStore.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<GameState>()), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenFileDoesNotExist()
    {
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json"))
            .Throws<FileNotFoundException>();

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("File not found");
        _mockGameStateStore.Verify(s => s.Clear(), Times.Never);
    }
    
    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenUnknownException()
    {
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json"))
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
            new GameState { Id = "first" },
            new GameState { Id = "second" },
            new GameState { Id = "third" }
        };
        var validJson = JsonSerializer.Serialize(games);
        _mockFileSystem.Setup(f => f.File.ReadAllText("dummyState.json")).Returns(validJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        _mockGameStateStore.Verify(s => s.Set("first", It.Is<GameState>(g => g.Id == "first")), Times.Once);
        _mockGameStateStore.Verify(s => s.Set("second", It.Is<GameState>(g => g.Id == "second")), Times.Once);
        _mockGameStateStore.Verify(s => s.Set("third", It.Is<GameState>(g => g.Id == "third")), Times.Once);
    }

    #endregion
}