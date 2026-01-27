using Clocktower.Server.Common.Services;
using Clocktower.Server.Common.UpdateModels;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Dto;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Game.Services;
using Clocktower.Server.Socket.Services;

namespace Clocktower.ServerTests.Game.Services;

[TestClass]
public class GameServiceTests
{
    private const string GameId = "game-id";
    private const string UserId = "123";
    private const string UserId2 = "456";
    private const string GuildId = "789";
    private const string DisplayName = "display name";
    private const string DisplayName2 = "display name2";

    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;
    private Mock<IGameBroadcastService> _mockGameBroadcastService = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<IScriptProvider> _mockScriptProvider = null!;
    private Mock<IIdGenerator> _mockIdGenerator = null!;

    private IGameService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBot = StrictMockFactory.Create<IDiscordBot>();
        _mockGamePerspectiveService = StrictMockFactory.Create<IGamePerspectiveService>();
        _mockGameBroadcastService = StrictMockFactory.Create<IGameBroadcastService>();
        _mockDiscordTownManager = StrictMockFactory.Create<IDiscordTownManager>();
        _mockScriptProvider = StrictMockFactory.Create<IScriptProvider>();
        _mockIdGenerator = StrictMockFactory.Create<IIdGenerator>();

        _sut = new GameService(
            _mockBot.Object,
            _mockGamePerspectiveService.Object,
            _mockDiscordTownManager.Object,
            _mockGameBroadcastService.Object,
            _mockScriptProvider.Object,
            _mockIdGenerator.Object
        );
    }

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

        _mockGamePerspectiveService.Setup(o => o.GetAll()).Returns(allGames);

        var result = _sut.GetGames();
        result.Should().BeEquivalentTo(allGames);

        _mockGamePerspectiveService.Verify(o => o.GetAll(), Times.Once);
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

        _mockGamePerspectiveService.Setup(o => o.GetUserGames(userId)).Returns(allGames);

        var result = _sut.GetPlayerGames(userId);
        result.Should().BeEquivalentTo(expected);

        _mockGamePerspectiveService.Verify(o => o.GetUserGames(userId), Times.Once);
    }

    #endregion

    #region GetGamePerspectives

    [TestMethod]
    public void GetGamePerspectives_ReturnsExpected_WhenStoreReturnsGame()
    {
        var gameId = CommonMethods.GetRandomString();
        var gamePerspective = CommonMethods.GetGamePerspective();

        _mockGamePerspectiveService.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns([gamePerspective]);

        var result = _sut.GetGamePerspectives(gameId);

        result.message.Should().Be("Game retrieved successfully");
        result.perspectives.Should().BeEquivalentTo([gamePerspective]);
        result.success.Should().BeTrue();
    }

    [TestMethod]
    public void GetGamePerspectives_ReturnsExpected_WhenStoreReturnsNull()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGamePerspectiveService.Setup(o => o.GetAllPerspectivesForGame(gameId)).Returns([]);

        var result = _sut.GetGamePerspectives(gameId);

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

        _mockGamePerspectiveService.Setup(o => o.RemoveGame(gameId)).Returns(true);

        var result = _sut.DeleteGame(gameId);

        result.success.Should().BeTrue();
        result.message.Should().Be($"Game deleted successfully");
    }

    [TestMethod]
    public void DeleteGame_ReturnsExpected_WhenStoreReturnsFalse()
    {
        var gameId = CommonMethods.GetRandomString();

        _mockGamePerspectiveService.Setup(o => o.RemoveGame(gameId)).Returns(false);

        var result = _sut.DeleteGame(gameId);

        result.success.Should().BeFalse();
        result.message.Should().Be($"Game ID '{gameId}' failed to be deleted");
    }

    #endregion

    #region StartNewGame

    [TestMethod]
    public void StartNewGame_ReturnsFalse_WhenGuildNotFound()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        _mockIdGenerator.Setup(o => o.GenerateGameId()).Returns(gameId);

        _mockBot.Setup(o => o.GetGuild(guildId)).Returns((IDiscordGuild)null!);

        var result = _sut.StartNewGame(guildId, userId);

        result.success.Should().BeFalse();
        result.gamePerspective.Should().BeNull();
        result.message.Should().Be("Couldn't find guild");
    }

    [TestMethod]
    public void StartNewGame_ReturnsFalse_WhenNoUserFound()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var guild = StrictMockFactory.Create<IDiscordGuild>();
        _mockIdGenerator.Setup(o => o.GenerateGameId()).Returns(gameId);

        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(userId)).Returns((IDiscordGuildUser)null!);

        var result = _sut.StartNewGame(guildId, userId);

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
        var expectedGameUser = new GameUser(userId) { UserType = UserType.StoryTeller };
        var expectedGamePerspective = CommonMethods.GetGamePerspective(gameId, userId: IGamePerspectiveStore.OmniscientKey, guildId, createdBy: expectedGameUser) with { Users = [expectedGameUser] };
        var guild = StrictMockFactory.Create<IDiscordGuild>();
        _mockIdGenerator.Setup(o => o.GenerateGameId()).Returns(gameId);

        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        var mockedUser = MockMaker.CreateMockDiscordGuildUser(userId, "name", "avatar");
        guild.Setup(o => o.GetUser(userId)).Returns(mockedUser);

        _mockGamePerspectiveService.Setup(o => o.InitializeGame(gameId, guildId, expectedGameUser)).Returns(expectedGamePerspective);
        _mockDiscordTownManager.Setup(o => o.UpdateUserIdentity(mockedUser.AsTownUser()));

        var result = _sut.StartNewGame(guildId, userId);

        _mockDiscordTownManager.Verify(o => o.UpdateUserIdentity(mockedUser.AsTownUser()), Times.Once);

        result.message.Should().Be("Game started successfully");
        result.gamePerspective.Should().BeEquivalentTo(expectedGamePerspective, options => options.Excluding(x => x.CreatedDate));
        result.gamePerspective.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.success.Should().BeTrue();
    }

    [TestMethod]
    public void StartNewGame_ReturnsExpected_WhenGamePerspectiveStoreSetReturnsFalse()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var gameId = CommonMethods.GetRandomString();
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var mockedUser = MockMaker.CreateMockDiscordGuildUser(userId, "name", "avatar");
        var guild = StrictMockFactory.Create<IDiscordGuild>();
        _mockIdGenerator.Setup(o => o.GenerateGameId()).Returns(gameId);
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(userId)).Returns(mockedUser);
        _mockGamePerspectiveService.Setup(o => o.InitializeGame(gameId, guildId, It.IsAny<GameUser>())).Returns((GamePerspective?)null);
        _mockDiscordTownManager.Setup(o => o.UpdateUserIdentity(mockedUser.AsTownUser()));

        var result = _sut.StartNewGame(guildId, userId);

        _mockDiscordTownManager.Verify(o => o.UpdateUserIdentity(mockedUser.AsTownUser()), Times.Once);
        result.success.Should().BeFalse();
        result.gamePerspective.Should().BeNull();
        result.message.Should().Be($"Perspective for user '{userId}' for game '{gameId}' already exists");
    }

    #endregion

    #region SetTime

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenGameNotFound()
    {
        const string gameId = "game-id";
        _mockGamePerspectiveService.Setup(o => o.GameExists(gameId)).Returns(false);

        var result = await _sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be("Game not found");
    }

    [TestMethod]
    [DynamicData(nameof(GetGameTimeValues))]
    public async Task SetTime_SetsTime_NotifyClients_WhenDataGood(GameTime gameTime)
    {
        const string gameId = "game-id";
        _mockGamePerspectiveService.Setup(o => o.GameExists(gameId)).Returns(true);
        _mockGamePerspectiveService.Setup(o => o.SetTime(gameId, gameTime));
        _mockGameBroadcastService.Setup(o => o.BroadcastTimeUpdate(gameId, gameTime)).Returns(Task.CompletedTask);

        var result = await _sut.SetTime(gameId, gameTime);

        result.message.Should().Be($"Time set to {gameTime}");
        result.success.Should().BeTrue();
        _mockGamePerspectiveService.Verify(o => o.SetTime(gameId, gameTime), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastTimeUpdate(gameId, gameTime), Times.Once);
    }

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenExceptionThrown()
    {
        const string gameId = "game-id";
        const string message = "message";
        _mockGamePerspectiveService.Setup(o => o.GameExists(gameId)).Throws(new Exception(message));

        var result = await _sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be(message);
    }

    #endregion

    #region GetAvailableGameUsers

    public void Setup_GetAvailableGameUsers(bool hasGame = true, bool hasGuild = true, (string id, bool inGame, bool isBot)[]? users = null)
    {
        var guildMock = StrictMockFactory.Create<IDiscordGuild>();
        users ??= [];
        var perspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow);

        var gameUsers = new List<GameUser>();
        var guildUsers = new List<IDiscordGuildUser>();
        foreach (var (id, inGame, isBot) in users)
        {
            var user = StrictMockFactory.Create<IDiscordGuildUser>();
            user.Setup(o => o.Id).Returns(id);
            user.Setup(o => o.IsBot).Returns(isBot);
            user.Setup(o => o.AsGameUser()).Returns(new GameUser(id));
            user.Setup(o => o.AsTownUser()).Returns(new TownUser(id, "name", "avatar"));

            guildUsers.Add(user.Object);

            if (inGame)
            {
                gameUsers.Add(user.Object.AsGameUser());
            }
        }


        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasGame ? perspective with { Users = gameUsers } : null);
        guildMock.Setup(o => o.Users).Returns(guildUsers);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guildMock.Object : null);
    }

    [TestMethod]
    public void GetAvailableUsers_ReturnsError_WhenGameNotFound()
    {
        Setup_GetAvailableGameUsers(hasGame: false);

        var result = _sut.GetAvailableGameUsers(GameId);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public void GetAvailableUsers_ReturnsError_WhenGuildNotFound()
    {
        Setup_GetAvailableGameUsers(hasGuild: false);

        var result = _sut.GetAvailableGameUsers(GameId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public void GetAvailableUsers_ReturnsOk_WhenNoUsers()
    {
        Setup_GetAvailableGameUsers(users: []);

        var result = _sut.GetAvailableGameUsers(GameId);

        var response = result.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject.ToArray();
        response.Should().NotBeNull();
        response.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAvailableUsers_ReturnsOk_WithExpectedUsers()
    {
        (string id, bool inGame, bool isBot)[] users =
        [
            new("1", false, false),
            new("2", true, false),
            new("3", false, true),
            new("4", true, true),
            new("5", false, false)
        ];
        Setup_GetAvailableGameUsers(users: users);

        var result = _sut.GetAvailableGameUsers(GameId);

        var response = result.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject.ToArray();
        response.Should().NotBeNull();
        response.Should().HaveCount(2);
        response.Should().Contain(x => x.Id == "1");
        response.Should().NotContain(x => x.Id == "2");
        response.Should().NotContain(x => x.Id == "3");
        response.Should().NotContain(x => x.Id == "4");
        response.Should().Contain(x => x.Id == "5");
    }

    #endregion

    #region AddUserToGame

    private void Setup_AddUserToGame(bool hasGame = true, bool hasGuild = true, bool hasUser = true)
    {
        var guildMock = StrictMockFactory.Create<IDiscordGuild>();
        var userMock = StrictMockFactory.Create<IDiscordGuildUser>();
        userMock.Setup(o => o.DisplayName).Returns(DisplayName!);
        var perspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow);
        var townUser = new TownUser(UserId, DisplayName!, "avatar");
        var gameUser = new GameUser(UserId);
        userMock.Setup(o => o.AsTownUser()).Returns(townUser);
        userMock.Setup(o => o.AsGameUser(perspective)).Returns(gameUser);

        _mockDiscordTownManager.Setup(o => o.UpdateUserIdentity(townUser));

        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasGame ? perspective : null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guildMock.Object : null);
        guildMock.Setup(o => o.GetUser(UserId)).Returns(hasUser ? userMock.Object : null);

        _mockGamePerspectiveService.Setup(o => o.GetNextAvailableSeatingPosition(GameId)).Returns(1);
        _mockGamePerspectiveService.Setup(o => o.AddUserToGame(GameId, It.IsAny<GameUser>())).Returns(true);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task AddUserToGame_ReturnsError_WhenGameNotFound()
    {
        Setup_AddUserToGame(hasGame: false);

        var result = await _sut.AddUserToGame(GameId, UserId);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task AddUserToGame_ReturnsError_WhenGuildNotFound()
    {
        Setup_AddUserToGame(hasGuild: false);

        var result = await _sut.AddUserToGame(GameId, UserId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task AddUserToGame_ReturnsError_WhenUserNotFound()
    {
        Setup_AddUserToGame(hasUser: false);

        var result = await _sut.AddUserToGame(GameId, UserId);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task AddUserToGame_ReturnsOk_AddsUserCorrectly()
    {
        Setup_AddUserToGame();

        var result = await _sut.AddUserToGame(GameId, UserId);

        _mockDiscordTownManager.Verify(o => o.UpdateUserIdentity(It.Is<TownUser>(townUser => townUser.Id == UserId)));
        _mockGamePerspectiveService.Verify(o => o.AddUserToGame(GameId, It.Is<GameUser>(gameUser =>
            gameUser.Id == UserId &&
            gameUser.UserType == UserType.Player &&
            gameUser.SeatingPosition == 1
        )));
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
        result.ShouldSucceedWith<string>($"{DisplayName} added to game: {GameId}");
    }

    #endregion

    #region RemoveUserFromGame

    private void Setup_RemoveUserFromGame(bool hasGame = true, bool hasGuild = true, bool hasUser = true)
    {
        var guildMock = StrictMockFactory.Create<IDiscordGuild>();
        var userMock = StrictMockFactory.Create<IDiscordGuildUser>();
        userMock.Setup(o => o.DisplayName).Returns(DisplayName!);
        var perspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow);
        var townUser = new TownUser(UserId, DisplayName!, "avatar");
        var gameUser = new GameUser(UserId);
        userMock.Setup(o => o.AsTownUser()).Returns(townUser);
        userMock.Setup(o => o.AsGameUser(perspective)).Returns(gameUser);

        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasGame ? perspective : null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guildMock.Object : null);
        guildMock.Setup(o => o.GetUser(UserId)).Returns(hasUser ? userMock.Object : null);

        _mockGamePerspectiveService.Setup(o => o.RemoveUserFromGame(GameId, UserId));
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task RemoveUserFromGame_ReturnsError_WhenGameNotFound()
    {
        Setup_RemoveUserFromGame(hasGame: false);

        var result = await _sut.RemoveUserFromGame(GameId, UserId);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task RemoveUserFromGame_ReturnsError_WhenGuildNotFound()
    {
        Setup_RemoveUserFromGame(hasGuild: false);

        var result = await _sut.RemoveUserFromGame(GameId, UserId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }


    [TestMethod]
    public async Task RemoveUserFromGame_ReturnsError_WhenUserNotFound()
    {
        Setup_RemoveUserFromGame(hasUser: false);

        var result = await _sut.RemoveUserFromGame(GameId, UserId);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }


    [TestMethod]
    public async Task RemoveUserFromGame_ReturnsOk_RemovesUserCorrectly()
    {
        Setup_RemoveUserFromGame();

        var result = await _sut.RemoveUserFromGame(GameId, UserId);

        _mockGamePerspectiveService.Verify(o => o.RemoveUserFromGame(GameId, UserId));
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
        result.ShouldSucceedWith<string>($"{DisplayName} removed from game: {GameId}");
    }

    #endregion

    #region RandomiseSeatingPositions

    [TestMethod]
    public async Task RandomiseSeatingPositions_ReturnsError_WhenGameNotFound()
    {
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns((GamePerspective)null!);

        var result = await _sut.RandomiseSeatingPositions(GameId);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task RandomiseSeatingPositions_UpdatesAllPlayersWithNewPositions()
    {
        var players = new[]
        {
            new GameUser("user1") { UserType = UserType.Player, IsPlaying = true, SeatingPosition = 0 },
            new GameUser("user2") { UserType = UserType.Player, IsPlaying = true, SeatingPosition = 1 },
            new GameUser("user3") { UserType = UserType.Player, IsPlaying = true, SeatingPosition = 2 }
        };
        var perspective = CommonMethods.GetGamePerspective(GameId) with { Users = players.ToList() };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(perspective);
        _mockGamePerspectiveService.Setup(o => o.UpdatePublicUser(GameId, It.IsAny<string>(), It.IsAny<PublicGameUserUpdate>())).Returns(true);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);

        var result = await _sut.RandomiseSeatingPositions(GameId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().OnlyHaveUniqueItems("each player should appear exactly once");
        result.Value.Should().Contain(["user1", "user2", "user3"]);

        foreach (var player in players)
        {
            _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, player.Id,
                It.Is<PublicGameUserUpdate>(u => u.SeatingPosition.HasValue)
            ), Times.Once);
        }

        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    #endregion

    #region SwapSeatingPositions

    [TestMethod]
    public async Task SwapSeatingPositions_ReturnsError_WhenGameNotFound()
    {
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns((GamePerspective)null!);

        var result = await _sut.SwapSeatingPositions(GameId, UserId + 1, UserId + 2);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SwapSeatingPositions_ReturnsError_WhenUser1NotFound()
    {
        var perspective = CommonMethods.GetGamePerspective(GameId) with { Users = [new GameUser(UserId + 2)] };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(perspective);

        var result = await _sut.SwapSeatingPositions(GameId, "non-existent", UserId + 2);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SwapSeatingPositions_ReturnsError_WhenUser2NotFound()
    {
        var perspective = CommonMethods.GetGamePerspective(GameId) with { Users = [new GameUser(UserId + 1)] };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(perspective);

        var result = await _sut.SwapSeatingPositions(GameId, UserId + 1, "non-existent");

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SwapSeatingPositions_SwapsPositionsCorrectly()
    {
        var u1 = new GameUser(UserId + 1) { SeatingPosition = 5 };
        var u2 = new GameUser(UserId + 2) { SeatingPosition = 10 };
        var perspective = CommonMethods.GetGamePerspective(GameId) with { Users = [u1, u2] };

        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(perspective);
        _mockGamePerspectiveService.Setup(o => o.UpdatePublicUser(GameId, It.IsAny<string>(), It.IsAny<PublicGameUserUpdate>())).Returns(true);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);

        var result = await _sut.SwapSeatingPositions(GameId, UserId + 1, UserId + 2);

        result.ShouldSucceedWith("Users swapped");

        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId + 1, new PublicGameUserUpdate { SeatingPosition = 10 }), Times.Once);
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId + 2, new PublicGameUserUpdate { SeatingPosition = 5 }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    #endregion

    private void Setup_GameUserStatusToggle(bool hasGame = true, bool hasGuild = true, bool hasUser = true, bool updateOccurred = true)
    {
        var guildMock = StrictMockFactory.Create<IDiscordGuild>();
        var userMock = StrictMockFactory.Create<IDiscordGuildUser>();
        userMock.Setup(o => o.DisplayName).Returns(DisplayName!);
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasGame ? CommonMethods.GetGamePerspective(GameId, guildId: GuildId) : null);
        guildMock.Setup(o => o.GetUser(UserId)).Returns(hasUser ? userMock.Object : null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guildMock.Object : null);
        _mockGamePerspectiveService.Setup(o => o.UpdatePublicUser(GameId, UserId, It.IsAny<PublicGameUserUpdate>())).Returns(updateOccurred);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    #region SetPlayerHasVoteToken

    [TestMethod]
    public async Task SetPlayerHasVoteToken_ReturnsError_WhenGameNotFound()
    {
        const bool hasVoteToken = true;
        Setup_GameUserStatusToggle(hasGame: false);

        var result = await _sut.SetPlayerHasVoteToken(GameId, UserId, hasVoteToken);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SetPlayerHasVoteToken_ReturnsError_WhenGuildNotFound()
    {
        const bool hasVoteToken = true;
        Setup_GameUserStatusToggle(hasGuild: false);

        var result = await _sut.SetPlayerHasVoteToken(GameId, UserId, hasVoteToken);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SetPlayerHasVoteToken_ReturnsError_WhenUserNotFound()
    {
        const bool hasVoteToken = true;
        Setup_GameUserStatusToggle(hasUser: false);

        var result = await _sut.SetPlayerHasVoteToken(GameId, UserId, hasVoteToken);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task SetPlayerHasVoteToken_ReturnsOk_WhenChangesHappen(bool hasVoteToken)
    {
        Setup_GameUserStatusToggle(updateOccurred: true);

        var result = await _sut.SetPlayerHasVoteToken(GameId, UserId, hasVoteToken);

        string expectedTokenStatus = hasVoteToken ? "has token" : "does not have token";
        result.ShouldSucceedWith($"{DisplayName} now {expectedTokenStatus}");
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId, new PublicGameUserUpdate { HasVoteToken = hasVoteToken }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task SetPlayerHasVoteToken_ReturnsOk_WhenNoChangesHappen(bool hasVoteToken)
    {
        Setup_GameUserStatusToggle(updateOccurred: false);

        var result = await _sut.SetPlayerHasVoteToken(GameId, UserId, hasVoteToken);

        string expectedTokenStatus = hasVoteToken ? "has token" : "does not have token";
        result.ShouldSucceedWith($"{DisplayName} already {expectedTokenStatus}");
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId, new PublicGameUserUpdate { HasVoteToken = hasVoteToken }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    #endregion

    #region SetPlayerIsDead

    [TestMethod]
    public async Task SetPlayerIsDead_ReturnsError_WhenGameNotFound()
    {
        const bool hasVoteToken = true;
        Setup_GameUserStatusToggle(hasGame: false);

        var result = await _sut.SetPlayerIsDead(GameId, UserId, hasVoteToken);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SetPlayerIsDead_ReturnsError_WhenGuildNotFound()
    {
        const bool hasVoteToken = true;
        Setup_GameUserStatusToggle(hasGuild: false);

        var result = await _sut.SetPlayerIsDead(GameId, UserId, hasVoteToken);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SetPlayerIsDead_ReturnsError_WhenUserNotFound()
    {
        const bool hasVoteToken = true;
        Setup_GameUserStatusToggle(hasUser: false);

        var result = await _sut.SetPlayerIsDead(GameId, UserId, hasVoteToken);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SetPlayerIsDead_ReturnsOk_WhenIsDeadFalse()
    {
        const bool isDead = false;
        Setup_GameUserStatusToggle(updateOccurred: true);

        var result = await _sut.SetPlayerIsDead(GameId, UserId, isDead);

        const string expectedDeadStatus = isDead ? "dead" : "alive";
        result.ShouldSucceedWith($"{DisplayName} is now {expectedDeadStatus}");
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId, new PublicGameUserUpdate { IsDead = isDead }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetPlayerIsDead_ReturnsOk_WhenIsDeadTrue()
    {
        const bool isDead = true;
        Setup_GameUserStatusToggle(updateOccurred: true);

        var result = await _sut.SetPlayerIsDead(GameId, UserId, isDead);

        const string expectedDeadStatus = isDead ? "dead" : "alive";
        result.ShouldSucceedWith($"{DisplayName} is now {expectedDeadStatus}");
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId, new PublicGameUserUpdate { IsDead = isDead, HasVoteToken = true }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }


    [TestMethod]
    public async Task SetPlayerIsDead_ReturnsOk_WhenNoChangesHappen()
    {
        const bool isDead = true;
        Setup_GameUserStatusToggle(updateOccurred: false);

        var result = await _sut.SetPlayerIsDead(GameId, UserId, isDead);

        const string expectedDeadStatus = isDead ? "dead" : "alive";
        result.ShouldSucceedWith($"{DisplayName} is already {expectedDeadStatus}");
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, UserId, new PublicGameUserUpdate { IsDead = isDead, HasVoteToken = true }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    #endregion

    #region SetScript

    private Script? Setup_SetScript(bool gameExists = true, bool getScriptSuccess = true, bool scriptExists = true)
    {
        _mockGamePerspectiveService.Setup(o => o.GameExists(GameId)).Returns(gameExists);

        var script = scriptExists ? new Script("Name", "Author", []) : null;
        var result = getScriptSuccess ? Result.Ok(script!) : Result.Fail<Script>(ErrorKind.Invalid, "code", "message");
        _mockScriptProvider.Setup(o => o.GetScriptAsync(It.IsAny<ScriptSelect>(), It.IsAny<string>())).ReturnsAsync(result);

        if (getScriptSuccess && scriptExists)
        {
            _mockGamePerspectiveService.Setup(o => o.SetScript(GameId, script!));
            _mockGameBroadcastService.Setup(o => o.BroadcastScriptUpdate(GameId, script!)).Returns(Task.CompletedTask);
        }

        return script;
    }

    [TestMethod]
    public async Task SetScript_ReturnsError_WhenGameMissing()
    {
        const ScriptSelect script = ScriptSelect.SectsAndViolets;
        const string json = "{}";

        Setup_SetScript(gameExists: false);

        var result = await _sut.SetScript(GameId, script, json);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SetScript_ReturnsError_WhenScriptFails()
    {
        const ScriptSelect script = ScriptSelect.SectsAndViolets;
        const string json = "{}";

        Setup_SetScript(getScriptSuccess: false);

        var result = await _sut.SetScript(GameId, script, json);

        result.ShouldFailWith(ErrorKind.Invalid, "code");
    }

    [TestMethod]
    public async Task SetScript_ReturnsError_WhenScriptNull()
    {
        const ScriptSelect script = ScriptSelect.SectsAndViolets;
        const string json = "{}";

        Setup_SetScript(scriptExists: false);

        var result = await _sut.SetScript(GameId, script, json);

        result.ShouldFailWith(ErrorKind.Invalid, "script.empty");
    }

    [TestMethod]
    public async Task SetScript_ReturnsOk_WhenScriptExists()
    {
        const ScriptSelect script = ScriptSelect.SectsAndViolets;
        const string json = "{}";

        var expected = Setup_SetScript();

        var result = await _sut.SetScript(GameId, script, json);

        result.ShouldSucceedWith(expected!);
        _mockGamePerspectiveService.Verify(o => o.SetScript(GameId, expected!), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastScriptUpdate(GameId, expected!), Times.Once);
    }

    #endregion

    #region CommitDraftRoles

    private void Setup_CommitDraftRoles(bool gameExists = true)
    {
        _mockGamePerspectiveService.Setup(o => o.GameExists(GameId)).Returns(gameExists);
        _mockGamePerspectiveService.Setup(o => o.CommitDraftRoles(GameId));
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task CommitDraftRoles_ReturnsError_WhenGameNotFound()
    {
        Setup_CommitDraftRoles(gameExists: false);

        var result = await _sut.CommitDraftRoles(GameId);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task CommitDraftRoles_ReturnsOk()
    {
        Setup_CommitDraftRoles();

        var result = await _sut.CommitDraftRoles(GameId);

        result.ShouldSucceedWith<string>($"Draft roles committed for game {GameId}");

        _mockGamePerspectiveService.Verify(o => o.CommitDraftRoles(GameId), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    #endregion

    #region SetPerspectiveRole

    private void Setup_SetPerspectiveRole(bool hasGame = true, bool hasGuild = true, bool hasUser = true, bool hasTargetUser = true, bool updated = true)
    {
        var guild = StrictMockFactory.Create<IDiscordGuild>();

        var targetUser = StrictMockFactory.Create<IDiscordGuildUser>();
        targetUser.Setup(o => o.DisplayName).Returns(DisplayName);
        var user = StrictMockFactory.Create<IDiscordGuildUser>();
        user.Setup(o => o.DisplayName).Returns(DisplayName2);

        var perspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow);

        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasGame ? perspective : null);

        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guild.Object : null);

        guild.Setup(o => o.GetUser(UserId)).Returns(hasTargetUser ? targetUser.Object : null);
        guild.Setup(o => o.GetUser(UserId2)).Returns(hasUser ? user.Object : null);
        _mockGamePerspectiveService.Setup(o => o.SetRoleOnPerspective(GameId, UserId2, UserId, It.IsAny<Role>())).Returns(updated);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsError_WhenGameNotFound()
    {
        Setup_SetPerspectiveRole(hasGame: false);

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsError_WhenGuildNotFound()
    {
        Setup_SetPerspectiveRole(hasGuild: false);

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsError_WhenUserNotFound()
    {
        Setup_SetPerspectiveRole(hasUser: false);

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsError_WhenTargetUserNotFound()
    {
        Setup_SetPerspectiveRole(hasTargetUser: false);

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsError_WhenRoleNotFound()
    {
        Setup_SetPerspectiveRole();

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, "invalid");

        result.ShouldFailWith(ErrorKind.NotFound, "role.not_found");
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsOk_RoleNotNull()
    {
        Setup_SetPerspectiveRole();

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, Role.Gunslinger.Id);

        result.ShouldSucceedWith<string>("display name now has the perspective role: Gunslinger");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnPerspective(GameId, UserId2, UserId, Role.Gunslinger), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsOk_RoleNull()
    {
        Setup_SetPerspectiveRole();

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, null);

        result.ShouldSucceedWith<string>("display name now has the perspective role: NONE");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnPerspective(GameId, UserId2, UserId, null), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsOk_RoleNull_NoChange()
    {
        Setup_SetPerspectiveRole(updated: false);

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, null);

        result.ShouldSucceedWith<string>("display name already has the perspective role: NONE");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnPerspective(GameId, UserId2, UserId, null), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task SetPerspectiveRole_ReturnsOk_RoleNotNull_NoChange()
    {
        Setup_SetPerspectiveRole(updated: false);

        var result = await _sut.SetPerspectiveRole(GameId, UserId2, UserId, Role.Gunslinger.Id);

        result.ShouldSucceedWith<string>("display name already has the perspective role: Gunslinger");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnPerspective(GameId, UserId2, UserId, Role.Gunslinger), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    #endregion

    #region SetDraftRole

    private void Setup_SetDraftRole(bool hasGame = true, bool hasGuild = true, bool hasTargetUser = true, bool updated = true)
    {
        var guild = StrictMockFactory.Create<IDiscordGuild>();
        var targetUser = StrictMockFactory.Create<IDiscordGuildUser>();
        targetUser.Setup(o => o.DisplayName).Returns(DisplayName);
        var perspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow);

        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasGame ? perspective : null);

        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guild.Object : null);

        guild.Setup(o => o.GetUser(UserId)).Returns(hasTargetUser ? targetUser.Object : null);
        _mockGamePerspectiveService.Setup(o => o.UpdateDraftRole(GameId, UserId, It.IsAny<Role>())).Returns(updated);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsError_WhenGameNotFound()
    {
        Setup_SetDraftRole(hasGame: false);

        var result = await _sut.SetDraftRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsError_WhenGuildNotFound()
    {
        Setup_SetDraftRole(hasGuild: false);

        var result = await _sut.SetDraftRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsError_WhenTargetUserNotFound()
    {
        Setup_SetDraftRole(hasTargetUser: false);

        var result = await _sut.SetDraftRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsError_WhenRoleNotFound()
    {
        Setup_SetDraftRole();

        var result = await _sut.SetDraftRole(GameId, UserId, "invalid");

        result.ShouldFailWith(ErrorKind.NotFound, "role.not_found");
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsOk_RoleNotNull()
    {
        Setup_SetDraftRole();

        var result = await _sut.SetDraftRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldSucceedWith<string>("display name now has the draft role: Gunslinger");
        _mockGamePerspectiveService.Verify(o => o.UpdateDraftRole(GameId, UserId, Role.Gunslinger), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsOk_RoleNull()
    {
        Setup_SetDraftRole();

        var result = await _sut.SetDraftRole(GameId, UserId, null);

        result.ShouldSucceedWith<string>("display name now has the draft role: NONE");
        _mockGamePerspectiveService.Verify(o => o.UpdateDraftRole(GameId, UserId, null), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsOk_RoleNull_NoChange()
    {
        Setup_SetDraftRole(updated: false);

        var result = await _sut.SetDraftRole(GameId, UserId, null);

        result.ShouldSucceedWith<string>("display name already has the draft role: NONE");
        _mockGamePerspectiveService.Verify(o => o.UpdateDraftRole(GameId, UserId, null), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task SetDraftRole_ReturnsOk_RoleNotNull_NoChange()
    {
        Setup_SetDraftRole(updated: false);

        var result = await _sut.SetDraftRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldSucceedWith<string>("display name already has the draft role: Gunslinger");
        _mockGamePerspectiveService.Verify(o => o.UpdateDraftRole(GameId, UserId, Role.Gunslinger), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    #endregion

    #region SetRole

    private void Setup_SetRole(bool hasGame = true, bool hasGuild = true, bool hasTargetUser = true, bool updated = true, Role? existingRole = null)
    {
        var guild = StrictMockFactory.Create<IDiscordGuild>();
        var targetUser = StrictMockFactory.Create<IDiscordGuildUser>();
        targetUser.Setup(o => o.DisplayName).Returns(DisplayName);
        var perspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow);

        if (existingRole != null)
        {
            perspective = perspective with { Users = [new GameUser(UserId) { Role = existingRole }] };
        }

        _mockGamePerspectiveService.Setup(o => o.GetPerspective(GameId, IGamePerspectiveStore.OmniscientKey)).Returns(hasGame ? perspective : null);

        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? guild.Object : null);

        guild.Setup(o => o.GetUser(UserId)).Returns(hasTargetUser ? targetUser.Object : null);


        _mockGamePerspectiveService.Setup(o => o.SetRoleOnAllPerspectives(GameId, UserId, It.IsAny<Role>())).Returns(updated);
        _mockGamePerspectiveService.Setup(o => o.UpdatePrivateUser(GameId, UserId, It.IsAny<PrivateGameUserUpdate>())).Returns(updated);
        _mockGameBroadcastService.Setup(o => o.BroadcastDiscordTownUpdate(GameId)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task SetRole_ReturnsError_WhenGameNotFound()
    {
        Setup_SetRole(hasGame: false);

        var result = await _sut.SetRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "game.not_found");
    }

    [TestMethod]
    public async Task SetRole_ReturnsError_WhenGuildNotFound()
    {
        Setup_SetRole(hasGuild: false);

        var result = await _sut.SetRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SetRole_ReturnsError_WhenTargetUserNotFound()
    {
        Setup_SetRole(hasTargetUser: false);

        var result = await _sut.SetRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldFailWith(ErrorKind.NotFound, "user.not_found");
    }

    [TestMethod]
    public async Task SetRole_ReturnsError_WhenRoleNotFound()
    {
        Setup_SetRole();

        var result = await _sut.SetRole(GameId, UserId, "invalid");

        result.ShouldFailWith(ErrorKind.NotFound, "role.not_found");
    }

    [TestMethod]
    public async Task SetRole_ReturnsOk_RoleNotNull()
    {
        Setup_SetRole();

        var result = await _sut.SetRole(GameId, UserId, Role.Empath.Id);

        result.ShouldSucceedWith<string>("display name now has the role: Empath");
        _mockGamePerspectiveService.Verify(o => o.UpdatePrivateUser(GameId, UserId, It.Is<PrivateGameUserUpdate>(u =>
            u.Role == Role.Empath &&
            !u.RemoveRole
        )), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetRole_ReturnsOk_RoleNull()
    {
        Setup_SetRole();

        var result = await _sut.SetRole(GameId, UserId, null);

        result.ShouldSucceedWith<string>("display name now has the role: NONE");
        _mockGamePerspectiveService.Verify(o => o.UpdatePrivateUser(GameId, UserId, It.Is<PrivateGameUserUpdate>(u =>
            u.Role == null &&
            u.RemoveRole
        )), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetRole_ReturnsOk_RoleNull_NoChange()
    {
        Setup_SetRole(updated: false);

        var result = await _sut.SetRole(GameId, UserId, null);

        result.ShouldSucceedWith<string>("display name already has the role: NONE");
        _mockGamePerspectiveService.Verify(o => o.UpdatePrivateUser(GameId, UserId, It.Is<PrivateGameUserUpdate>(u =>
            u.Role == null &&
            u.RemoveRole
        )), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task SetRole_ReturnsOk_RoleNotNull_NoChange()
    {
        Setup_SetRole(updated: false);

        var result = await _sut.SetRole(GameId, UserId, Role.Empath.Id);

        result.ShouldSucceedWith<string>("display name already has the role: Empath");
        _mockGamePerspectiveService.Verify(o => o.UpdatePrivateUser(GameId, UserId, It.Is<PrivateGameUserUpdate>(u =>
            u.Role == Role.Empath &&
            !u.RemoveRole
        )), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task SetRole_ReturnsOk_ChangeToTraveller()
    {
        Setup_SetRole();

        var result = await _sut.SetRole(GameId, UserId, Role.Gunslinger.Id);

        result.ShouldSucceedWith<string>("display name now has the role: Gunslinger");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnAllPerspectives(GameId, UserId, Role.Gunslinger), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task SetRole_ReturnsOk_ChangeFromTraveller_OtherRole()
    {
        Setup_SetRole(existingRole: Role.Gunslinger);

        var result = await _sut.SetRole(GameId, UserId, Role.Empath.Id);

        result.ShouldSucceedWith<string>("display name now has the role: Empath");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnAllPerspectives(GameId, UserId, null), Times.Once);
        _mockGamePerspectiveService.Verify(o => o.UpdatePrivateUser(GameId, UserId, It.Is<PrivateGameUserUpdate>(u =>
            u.Role == Role.Empath &&
            !u.RemoveRole
        )), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }


    [TestMethod]
    public async Task SetRole_ReturnsOk_ChangeFromTraveller_Null()
    {
        Setup_SetRole(existingRole: Role.Gunslinger);

        var result = await _sut.SetRole(GameId, UserId, null);

        result.ShouldSucceedWith<string>("display name now has the role: NONE");
        _mockGamePerspectiveService.Verify(o => o.SetRoleOnAllPerspectives(GameId, UserId, null), Times.Once);
        _mockGamePerspectiveService.Verify(o => o.UpdatePrivateUser(GameId, UserId, It.IsAny<PrivateGameUserUpdate>()), Times.Never);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    #endregion

    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
}