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
public class GamePerspectiveServiceTests
{
    private const string DummyJsonFile = "dummyState.json";

    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<IGamePerspectiveStore> _mockGamePerspectiveStore = null!;
    private Mock<IFileSystem> _mockFileSystem = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockGamePerspectiveStore = new Mock<IGamePerspectiveStore>();
        _mockFileSystem = new Mock<IFileSystem>();
        _mockNotificationService = new Mock<INotificationService>();
    }

    private IGamePerspectiveService Sut => new GamePerspectiveService(_mockBot.Object, _mockGamePerspectiveStore.Object, _mockFileSystem.Object, _mockNotificationService.Object);

    #region GetGames

    [TestMethod]
    public void GetGames_CallsGamePerspectiveStore()
    {
        GamePerspective[] allGames =
        [
            CommonMethods.GetGamePerspective(),
            CommonMethods.GetGamePerspective(),
            CommonMethods.GetGamePerspective()
        ];

        _mockGamePerspectiveStore.Setup(o => o.GetAll()).Returns(allGames);

        var result = Sut.GetGames();
        result.Should().BeEquivalentTo(allGames);

        _mockGamePerspectiveStore.Verify(o => o.GetAll(), Times.Once);
    }

    #endregion

    #region GetPlayerGames

    [TestMethod]
    public void GetPlayerGames_CallsGamePerspectiveStore()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        GamePerspective[] allGames =
        [
            CommonMethods.GetGamePerspective(creatorId: userId),
            CommonMethods.GetGamePerspective(creatorId: userId),
            CommonMethods.GetGamePerspective(creatorId: userId)
        ];
        var expected = allGames.Select(o => new MiniGamePerspective(o.Id, o.CreatedBy, o.CreatedDate));

        _mockGamePerspectiveStore.Setup(o => o.GetUserGames(userId)).Returns(allGames);

        var result = Sut.GetPlayerGames(userId);
        result.Should().BeEquivalentTo(expected);

        _mockGamePerspectiveStore.Verify(o => o.GetUserGames(userId), Times.Once);
    }

    #endregion

    #region GetGame

    [TestMethod]
    public void GetGame_ReturnsExpected_WhenStoreReturnsGame()
    {
        var gameId = CommonMethods.GetRandomString();
        var gamePerspective = CommonMethods.GetGamePerspective();

        _mockGamePerspectiveStore.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns([gamePerspective]);

        var result = Sut.GetGame(gameId);

        result.success.Should().BeTrue();
        result.perspectives.Should().BeEquivalentTo([gamePerspective]);
        result.message.Should().Be("Game retrieved successfully");
    }

    [TestMethod]
    public void GetGame_ReturnsExpected_WhenStoreReturnsNull()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGamePerspectiveStore.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns([]);

        var result = Sut.GetGame(gameId);

        result.success.Should().BeFalse();
        result.perspectives.Should().BeEmpty();
        result.message.Should().Be($"Game ID '{gameId}' not found");
    }

    #endregion

    #region DeleteGame

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsTrue()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGamePerspectiveStore.Setup(o => o.RemoveGame(gameId)).Returns(true);

        var result = Sut.DeleteGame(gameId);

        result.success.Should().BeTrue();
        result.message.Should().Be($"Game deleted successfully");
    }

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGamePerspectiveStore.Setup(o => o.RemoveGame(gameId)).Returns(false);

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
        result.gamePerspective.Should().BeNull();
        result.message.Should().Be("Couldn't find user");
    }

    [TestMethod]
    public void StartNewGame_ReturnsExpected_WhenGamePerspectiveStoreSetReturnsTrue()
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
        var expectedGamePerspective = CommonMethods.GetGamePerspective(gameId, userId: userId, guildId, createdBy: expectedGameUser) with { Users = [expectedGameUser] };

        var mockedUser = MockMaker.CreateMockDiscordUser(userId, userName, userAvatarUrl);
        _mockBot.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGamePerspectiveStore.Setup(o => o.Set(It.Is<GamePerspective>(g => g.Id == gameId && g.UserId == userId))).Returns(true);

        var result = Sut.StartNewGame(guildId, gameId, userId);

        result.success.Should().BeTrue();
        result.gamePerspective.Should().BeEquivalentTo(expectedGamePerspective, options => options
            .Excluding(x => x.CreatedDate));
        result.gamePerspective.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.message.Should().Be("Game started successfully");
    }

    [TestMethod]
    public void StartNewGame_ReturnsExpected_WhenGamePerspectiveStoreSetReturnsFalse()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var userName = CommonMethods.GetRandomString();
        var userAvatarUrl = CommonMethods.GetRandomString();

        var mockedUser = MockMaker.CreateMockDiscordUser(userId, userName, userAvatarUrl);
        _mockBot.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGamePerspectiveStore.Setup(o => o.Set(It.Is<GamePerspective>(g => g.Id == gameId))).Returns(false);

        var result = Sut.StartNewGame(guildId, gameId, userId);

        result.success.Should().BeFalse();
        result.gamePerspective.Should().BeNull();
        result.message.Should().Be($"Perspective for user '{userId}' for game '{gameId}' already exists");
    }

    #endregion

    #region LoadDummyData

    [TestMethod]
    public void LoadDummyData_ReturnsSuccess_WhenValidJsonFileExists()
    {
        var validJson = JsonSerializer.Serialize(new[]
        {
            CommonMethods.GetGamePerspective("game1", "guild1"),
            CommonMethods.GetGamePerspective("game2", "guild2")
        });

        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(validJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        result.message.Should().Be("Loaded dummy data");
        _mockGamePerspectiveStore.Verify(s => s.Clear(), Times.Once);
        _mockGamePerspectiveStore.Verify(s => s.Set(It.Is<GamePerspective>(o => o.Id == "game1")), Times.Once);
        _mockGamePerspectiveStore.Verify(s => s.Set(It.Is<GamePerspective>(o => o.Id == "game2")), Times.Once);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenJsonDeserializationFails()
    {
        const string invalidJson = "{ invalid json }";
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(invalidJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to deserialize json");
        _mockGamePerspectiveStore.Verify(s => s.Clear(), Times.Never);
        _mockGamePerspectiveStore.Verify(s => s.Set(It.IsAny<GamePerspective>()), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenJsonDeserializesToNull()
    {
        const string nullJson = "null";
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(nullJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to deserialize json");
        _mockGamePerspectiveStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ClearsStoreBeforeLoading()
    {
        var validJson = JsonSerializer.Serialize(new[] { CommonMethods.GetGamePerspective() });
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(validJson);

        Sut.LoadDummyData();

        var invocations = _mockGamePerspectiveStore.Invocations.ToList();
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
        _mockGamePerspectiveStore.Verify(s => s.Clear(), Times.Once);
        _mockGamePerspectiveStore.Verify(s => s.Set(It.IsAny<GamePerspective>()), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenFileDoesNotExist()
    {
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile))
            .Throws<FileNotFoundException>();

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be("File not found");
        _mockGamePerspectiveStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_ReturnsFalse_WhenUnknownException()
    {
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile))
            .Throws(new Exception("Custom error message"));

        var result = Sut.LoadDummyData();

        result.success.Should().BeFalse();
        result.message.Should().Be($"Error loading dummy data: Custom error message");
        _mockGamePerspectiveStore.Verify(s => s.Clear(), Times.Never);
    }

    [TestMethod]
    public void LoadDummyData_LoadsMultipleGames_InCorrectOrder()
    {
        var games = new[]
        {
            CommonMethods.GetGamePerspective("first"),
            CommonMethods.GetGamePerspective("second"),
            CommonMethods.GetGamePerspective("third")
        };
        var validJson = JsonSerializer.Serialize(games);
        _mockFileSystem.Setup(f => f.File.ReadAllText(DummyJsonFile)).Returns(validJson);

        var result = Sut.LoadDummyData();

        result.success.Should().BeTrue();
        _mockGamePerspectiveStore.Verify(s => s.Set(It.Is<GamePerspective>(g => g.Id == "first")), Times.Once);
        _mockGamePerspectiveStore.Verify(s => s.Set(It.Is<GamePerspective>(g => g.Id == "second")), Times.Once);
        _mockGamePerspectiveStore.Verify(s => s.Set(It.Is<GamePerspective>(g => g.Id == "third")), Times.Once);
    }

    #endregion

    #region SetTime

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenGameNotFound()
    {
        const string gameId = "game-id";
        _mockGamePerspectiveStore.Setup(o => o.GameExists(gameId)).Returns(false);

        var result = await Sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be("Game not found");
    }

    [TestMethod]
    [DynamicData(nameof(GetGameTimeValues))]
    public async Task SetTime_SetsTime_NotifyClients_WhenDataGood(GameTime gameTime)
    {
        const string gameId = "game-id";
        _mockGamePerspectiveStore.Setup(o => o.GameExists(gameId)).Returns(true);

        var result = await Sut.SetTime(gameId, gameTime);

        _mockGamePerspectiveStore.Verify(o => o.SetTime(gameId, gameTime), Times.Once);
        _mockNotificationService.Verify(o => o.BroadcastTownTime(gameId, gameTime), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be($"Time set to {gameTime}");
    }

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenExceptionThrown()
    {
        const string gameId = "game-id";
        const string message = "message";
        _mockGamePerspectiveStore.Setup(o => o.GameExists(gameId)).Throws(new Exception(message));

        var result = await Sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be(message);
    }

    #endregion

    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
}