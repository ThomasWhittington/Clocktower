using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Data.Stores;

[TestClass]
public class GamePerspectiveStoreTests
{
    private const string GameId1 = "_game1";
    private const string GameId2 = "_game2";
    private const string GameId3 = "_game3";
    private const string UserId1 = "123";
    private const string UserId2 = "456";
    private const string UserId3 = "789";
    private const string GuildId = "123456789";
    private IGamePerspectiveStore _sut = null!;

    private GamePerspective _game1 = null!;
    private GamePerspective _game2 = null!;
    private GamePerspective _game3 = null!;

    [TestInitialize]
    public void SetUp()
    {
        _game1 = CommonMethods.GetGamePerspective(GameId1, guildId: GuildId);
        _game2 = CommonMethods.GetGamePerspective(GameId2, guildId: GuildId);
        _game3 = CommonMethods.GetGamePerspective(GameId3, guildId: GuildId);
        _sut = new GamePerspectiveStore();
    }

    [TestMethod]
    public void GameExists_ReturnsFalse_WhenGameNotFound()
    {
        var result = _sut.GameExists("missing-game");

        result.Should().BeFalse();
    }

    [TestMethod]
    public void GameExists_ReturnsTrue_WhenGameFound()
    {
        _sut.Set(_game1 with { UserId = UserId1 });

        var result = _sut.GameExists(GameId1);

        result.Should().BeTrue();
    }

    [TestMethod]
    public void Set_WhenGameDoesNotExist_ReturnsTrue()
    {
        var game = _game1 with { UserId = UserId1 };
        var result = _sut.Set(game);

        result.Should().BeTrue();
        _sut.Get(GameId1, UserId1).Should().BeEquivalentTo(game);
    }


    [TestMethod]
    public void Set_WhenPerspectiveAlreadyExists_ReturnsFalse()
    {
        var game = _game1 with { UserId = UserId1 };
        _sut.Set(game);

        var result = _sut.Set(CommonMethods.GetGamePerspective(GameId1) with { UserId = UserId1 });

        result.Should().BeFalse();
        _sut.Get(GameId1, UserId1).Should().BeEquivalentTo(game);
    }

    [TestMethod]
    public void Get_WhenGameDoesNotExist_ReturnsNull()
    {
        var result = _sut.Get("nonexistent", UserId1);

        result.Should().BeNull();
    }


    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });
        _sut.Set(_game2 with { UserId = UserId2 });

        _sut.Clear();

        _sut.Get(GameId1, UserId1).Should().BeNull();
        _sut.Get(GameId1, UserId2).Should().BeNull();
        _sut.Get(GameId2, UserId1).Should().BeNull();
        _sut.Get(GameId2, UserId2).Should().BeNull();
    }

    [TestMethod]
    public void RemovePerspective_RemovesOnlySelectedPerspective()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });

        _sut.RemovePerspective(GameId1, UserId1);

        _sut.Get(GameId1, UserId1).Should().BeNull();
        _sut.Get(GameId1, UserId2).Should().NotBeNull();
    }

    [TestMethod]
    public void RemoveGame_RemovesAllPerspectivesFromGame()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });
        _sut.Set(_game2 with { UserId = UserId2 });

        var result = _sut.RemoveGame(GameId1);

        result.Should().BeTrue();
        _sut.Get(GameId1, UserId1).Should().BeNull();
        _sut.Get(GameId1, UserId2).Should().BeNull();
        _sut.Get(GameId2, UserId1).Should().NotBeNull();
        _sut.Get(GameId2, UserId2).Should().NotBeNull();
    }

    [TestMethod]
    [DynamicData(nameof(GetGameTimeValues))]
    public void SetTime_UpdatesGameTimeForAllPerspectivesInGame(GameTime gameTime)
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });

        var game2GameTime = _sut.Get(GameId2, UserId1)!.GameTime;

        _sut.SetTime(GameId1, gameTime);

        _sut.Get(GameId1, UserId1)!.GameTime.Should().Be(gameTime);
        _sut.Get(GameId1, UserId2)!.GameTime.Should().Be(gameTime);
        _sut.Get(GameId2, UserId1)!.GameTime.Should().Be(game2GameTime);
    }

    [TestMethod]
    public void AddUserToGame_ChangesNothing_WhenGameNotFound()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });

        var user = CommonMethods.GetRandomGameUser(UserId3);

        _sut.AddUserToGame(GameId3, user);

        _sut.Get(GameId1, UserId1)!.Users.Should().NotContain(o => o.Id == UserId3);
        _sut.Get(GameId1, UserId2)!.Users.Should().NotContain(o => o.Id == UserId3);
        _sut.Get(GameId2, UserId1)!.Users.Should().NotContain(o => o.Id == UserId3);
    }

    [TestMethod]
    public void AddUserToGame_AddsUserForAllPerspectivesInGame()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });

        var user = CommonMethods.GetRandomGameUser(UserId3);

        _sut.AddUserToGame(GameId1, user);

        _sut.Get(GameId1, UserId1)!.Users.Should().Contain(o => o.Id == UserId3);
        _sut.Get(GameId1, UserId2)!.Users.Should().Contain(o => o.Id == UserId3);
        _sut.Get(GameId2, UserId1)!.Users.Should().NotContain(o => o.Id == UserId3);
    }


    [TestMethod]
    public void AddUserToGame_ChangesNothing_WhenUserAlreadyInGame()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1);
        _sut.Set(_game1 with { UserId = UserId1, Users = [user] });
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        _sut.AddUserToGame(GameId1, user);

        _sut.Get(GameId1, UserId1)!.Users.Should().Contain(o => o.Id == UserId1);
        _sut.Get(GameId1, UserId1)!.Users.Should().HaveCount(1);
        _sut.Get(GameId1, UserId2)!.Users.Should().Contain(o => o.Id == UserId1);
        _sut.Get(GameId1, UserId2)!.Users.Should().HaveCount(1);
    }

    [TestMethod]
    public void RemoveUserFromGame_ChangesNothing_WhenGameNotFound()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });

        _sut.RemoveUserFromGame(GameId3, UserId3);

        _sut.Get(GameId1, UserId1)!.Users.Should().NotContain(o => o.Id == UserId3);
        _sut.Get(GameId1, UserId2)!.Users.Should().NotContain(o => o.Id == UserId3);
        _sut.Get(GameId2, UserId1)!.Users.Should().NotContain(o => o.Id == UserId3);
    }

    [TestMethod]
    public void RemoveUserFromGame_RemovesUserForAllPerspectivesInGame()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1);
        _sut.Set(_game1 with { UserId = UserId1, Users = [user] });
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });
        _sut.Set(_game2 with { UserId = UserId1, Users = [user] });

        _sut.RemoveUserFromGame(GameId1, UserId1);

        _sut.Get(GameId1, UserId1).Should().BeNull();
        _sut.Get(GameId2, UserId1).Should().NotBeNull();
        _sut.Get(GameId1, UserId2)!.Users.Should().NotContain(o => o.Id == UserId1);
    }


    [TestMethod]
    public void RemoveUserFromGame_ChangesNothing_WhenUserNotInGame()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1);
        _sut.Set(_game1 with { UserId = UserId1, Users = [user] });
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        _sut.RemoveUserFromGame(GameId1, UserId3);

        _sut.Get(GameId1, UserId1)!.Users.Should().Contain(o => o.Id == UserId1);
        _sut.Get(GameId1, UserId1)!.Users.Should().HaveCount(1);
        _sut.Get(GameId1, UserId2)!.Users.Should().Contain(o => o.Id == UserId1);
        _sut.Get(GameId1, UserId2)!.Users.Should().HaveCount(1);
    }


    [TestMethod]
    public void GetAll_ReturnsAll()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game2 with { UserId = UserId1 });
        _sut.Set(_game3 with { UserId = UserId3 });

        var result = _sut.GetAll().ToList();

        result.Should().Contain(o => o.Id == GameId1);
        result.Should().Contain(o => o.Id == GameId2);
        result.Should().Contain(o => o.Id == GameId3);
    }

    [TestMethod]
    public void GetFirstPerspective_ReturnsFirst()
    {
        var game1 = _game1 with { UserId = UserId1 };
        var game1Day = _game1 with { UserId = UserId2, GameTime = GameTime.Day };
        var game1Night = _game1 with { UserId = UserId3, GameTime = GameTime.Night };
        _sut.Set(game1);
        _sut.Set(game1Day);
        _sut.Set(game1Night);

        var result = _sut.GetFirstPerspective(GameId1);

        result.Should().BeOneOf(game1, game1Day, game1Night);
    }

    [TestMethod]
    public void GetAllPerspectivesForGame_GetsAllPerspectivesForGame()
    {
        var game1 = _game1 with { UserId = UserId1 };
        var game1Day = _game1 with { UserId = UserId2, GameTime = GameTime.Day };
        var game1Night = _game1 with { UserId = UserId3, GameTime = GameTime.Night };
        _sut.Set(game1);
        _sut.Set(game1Day);
        _sut.Set(game1Night);
        _sut.Set(_game2 with { UserId = UserId3 });


        var result = _sut.GetAllPerspectivesForGame(GameId1).ToArray();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo([game1, game1Day, game1Night]);
        result.Should().NotContain(_game2);
    }

    [TestMethod]
    public void GetGuildGames_ReturnsGuildGames()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game2 with { UserId = UserId1 });
        _sut.Set(_game3 with { UserId = UserId1 });
        _sut.Set(CommonMethods.GetGamePerspective("game4", UserId3, guildId: "987"));

        var result = _sut.GetGuildGameIds(GuildId).ToList();

        result.Should().HaveCount(3);
        result.Should().Contain(GameId1);
        result.Should().Contain(GameId2);
        result.Should().Contain(GameId3);
        result.Should().NotContain("game4");
    }

    [TestMethod]
    public void GetUserGames_ReturnsUserGames()
    {
        _sut.Set(_game1 with { UserId = UserId2 });
        _sut.Set(_game2 with { UserId = UserId1 });
        _sut.Set(_game3 with { UserId = UserId1 });

        var result = _sut.GetUserGames(UserId1).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == GameId2);
        result.Should().Contain(o => o.Id == GameId3);
    }

    #region UpdateUser

    [TestMethod]
    public void UpdateUser_DoesNotChange_WhenNoUserFound()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        var user = CommonMethods.GetRandomGameUser(UserId3);
        _sut.AddUserToGame(GameId1, user);

        var result = _sut.UpdateUser(GameId1, UserId1);

        result.Should().BeFalse();
        _sut.Get(GameId1, UserId1)!.Users.Should().Contain(user);
        _sut.Get(GameId1, UserId2)!.Users.Should().Contain(user);
    }

    [TestMethod]
    public void UpdateUser_ReturnsOriginal_NoChangesRequested()
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        var user = CommonMethods.GetRandomGameUser(UserId3);
        _sut.AddUserToGame(GameId1, user);

        var result = _sut.UpdateUser(GameId1, UserId3, userType: user.UserType, isPlaying: user.IsPlaying);

        result.Should().BeFalse();
        _sut.Get(GameId1, UserId1)!.Users.Should().Contain(user);
        _sut.Get(GameId1, UserId2)!.Users.Should().Contain(user);
    }

    [TestMethod]
    [DynamicData(nameof(GetUserTypeValues))]
    public void UpdateUser_Updates_UserType(UserType userType)
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        var user = CommonMethods.GetRandomGameUser(UserId3);
        if (userType == UserType.Unknown) user = user with { UserType = UserType.Player };
        _sut.AddUserToGame(GameId1, user);

        var result = _sut.UpdateUser(GameId1, UserId3, userType: userType);

        result.Should().BeTrue();
        _sut.Get(GameId1, UserId1)!.Users[0].UserType.Should().Be(userType);
        _sut.Get(GameId1, UserId2)!.Users[0].UserType.Should().Be(userType);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void UpdateUser_Updates_IsPlaying(bool isPlaying)
    {
        _sut.Set(_game1 with { UserId = UserId1 });
        _sut.Set(_game1 with { UserId = UserId2 });
        var user = CommonMethods.GetRandomGameUser(UserId3) with { IsPlaying = !isPlaying };
        _sut.AddUserToGame(GameId1, user);

        var result = _sut.UpdateUser(GameId1, UserId3, isPlaying: isPlaying);

        result.Should().BeTrue();
        _sut.Get(GameId1, UserId1)!.Users[0].IsPlaying.Should().Be(isPlaying);
        _sut.Get(GameId1, UserId2)!.Users[0].IsPlaying.Should().Be(isPlaying);
    }

    #endregion


    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
    private static IEnumerable<object[]> GetUserTypeValues() => TestDataProvider.GetAllEnumValues<UserType>();
}