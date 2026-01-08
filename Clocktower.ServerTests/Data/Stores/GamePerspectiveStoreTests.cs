using Clocktower.Server.Common.UpdateModels;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;

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
    private const string UserId4 = "987";
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
        var user = CommonMethods.GetRandomGameUser(UserId1) with { SeatingPosition = 0 };
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
        var user1 = CommonMethods.GetRandomGameUser(UserId1) with { SeatingPosition = 0 };
        var user2 = CommonMethods.GetRandomGameUser(UserId2) with { SeatingPosition = 1 };
        _sut.Set(_game1 with { UserId = UserId1, Users = [user1] });
        _sut.Set(_game1 with { UserId = UserId2, Users = [user1] });
        _sut.Set(_game2 with { UserId = UserId1, Users = [user2] });

        _sut.RemoveUserFromGame(GameId3, UserId3);

        _sut.Get(GameId1, UserId1).Should().NotBeNull();
        _sut.Get(GameId1, UserId2).Should().NotBeNull();
        _sut.Get(GameId2, UserId1).Should().NotBeNull();
        _sut.Get(GameId1, UserId1)!.Users.Should().ContainSingle().Which.Id.Should().Be(UserId1);
        _sut.Get(GameId1, UserId2)!.Users.Should().ContainSingle().Which.Id.Should().Be(UserId1);
        _sut.Get(GameId2, UserId1)!.Users.Should().ContainSingle().Which.Id.Should().Be(UserId2);
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
    public void RemoveUserFromGame_ReIndexesRemainingUsersCorrectly()
    {
        var user0 = CommonMethods.GetRandomGameUser(UserId1) with { UserType = UserType.Player, SeatingPosition = 0, IsPlaying = true };
        var user1 = CommonMethods.GetRandomGameUser(UserId2) with { UserType = UserType.Player, SeatingPosition = 1, IsPlaying = true };
        var user2 = CommonMethods.GetRandomGameUser(UserId3) with { UserType = UserType.Player, SeatingPosition = 2, IsPlaying = true };

        _sut.Set(_game1 with { UserId = UserId1, Users = [user0, user1, user2] });
        _sut.Set(_game1 with { UserId = UserId3, Users = [user0, user1, user2] });

        _sut.RemoveUserFromGame(GameId1, UserId2);

        var perspective1 = _sut.Get(GameId1, UserId1);
        perspective1.Should().NotBeNull();
        perspective1!.Users.Should().HaveCount(2);

        perspective1.Users.First(u => u.Id == UserId1).SeatingPosition.Should().Be(0);
        perspective1.Users.First(u => u.Id == UserId3).SeatingPosition.Should().Be(1);

        var perspective3 = _sut.Get(GameId1, UserId3);
        perspective3!.Users.First(u => u.Id == UserId3).SeatingPosition.Should().Be(1);
    }

    [TestMethod]
    public void RemoveUserFromGame_MaintainsZeroIndex_WhenFirstUserRemoved()
    {
        var user0 = CommonMethods.GetRandomGameUser(UserId1) with { UserType = UserType.Player, SeatingPosition = 0, IsPlaying = true };
        var user1 = CommonMethods.GetRandomGameUser(UserId2) with { UserType = UserType.Player, SeatingPosition = 1, IsPlaying = true };
        _sut.Set(_game1 with { UserId = UserId2, Users = [user0, user1] });

        _sut.RemoveUserFromGame(GameId1, UserId1);

        var perspective = _sut.Get(GameId1, UserId2);
        perspective!.Users.Should().HaveCount(1);
        perspective.Users.Single().SeatingPosition.Should().Be(0, "the remaining user should shift from position 1 to 0");
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

    #region UpdatePublicUser

    [TestMethod]
    public void UpdatePublicUser_ReturnsFalse_WhenUserNotFound()
    {
        _sut.Set(_game1 with { UserId = UserId1 });

        var result = _sut.UpdatePublicUser(GameId1, "missing-user", new GameUserUpdate());

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_ReturnsFalse_WhenNoChangesProvided()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1);
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new GameUserUpdate());

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_ReturnsFalse_WhenAllPropertiesAlreadyMatch()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1) with
        {
            UserType = UserType.Player,
            IsPlaying = true
        };
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new GameUserUpdate
        {
            UserType = UserType.Player,
            IsPlaying = true
        });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_UpdatesSingleProperty()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1) with { IsDead = false };
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new GameUserUpdate { IsDead = true });

        result.Should().BeTrue();
        _sut.Get(GameId1, UserId2)!.Users[0].IsDead.Should().BeTrue();
    }

    [TestMethod]
    public void UpdatePublicUser_UpdatesMultipleProperties()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1) with
        {
            IsDead = false,
            IsMarked = false,
            HasVoteToken = false
        };
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new GameUserUpdate
        {
            IsDead = true,
            IsMarked = true,
            HasVoteToken = true
        });

        result.Should().BeTrue();
        var updatedUser = _sut.Get(GameId1, UserId2)!.Users[0];
        updatedUser.IsDead.Should().BeTrue();
        updatedUser.IsMarked.Should().BeTrue();
        updatedUser.HasVoteToken.Should().BeTrue();
    }

    [TestMethod]
    public void UpdatePublicUser_UpdatesAllPerspectives()
    {
        var user = CommonMethods.GetRandomGameUser(UserId3) with { IsDead = false };
        _sut.Set(_game1 with { UserId = UserId1, Users = [user] });
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        _sut.UpdatePublicUser(GameId1, UserId3, new GameUserUpdate { IsDead = true });

        _sut.Get(GameId1, UserId1)!.Users[0].IsDead.Should().BeTrue();
        _sut.Get(GameId1, UserId2)!.Users[0].IsDead.Should().BeTrue();
    }

    [TestMethod]
    public void UpdatePublicUser_OnlyUpdatesChangedProperties()
    {
        var user = CommonMethods.GetRandomGameUser(UserId1) with
        {
            UserType = UserType.Player,
            IsDead = false,
            SeatingPosition = 5
        };
        _sut.Set(_game1 with { UserId = UserId2, Users = [user] });

        _sut.UpdatePublicUser(GameId1, UserId1, new GameUserUpdate
        {
            UserType = UserType.Player,
            IsDead = true
        });

        var updatedUser = _sut.Get(GameId1, UserId2)!.Users[0];
        updatedUser.UserType.Should().Be(UserType.Player);
        updatedUser.IsDead.Should().BeTrue();
        updatedUser.SeatingPosition.Should().Be(5);
    }

    #endregion

    #region GetNextAvailableSeatingPosition

    [TestMethod]
    public void GetNextAvailableSeatingPosition_Returns0ForFirstPlayer()
    {
        var user1 = CommonMethods.GetRandomGameUser(UserId3) with { UserType = UserType.StoryTeller, SeatingPosition = 0 };
        _sut.Set(_game1 with { Users = [user1] });

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(0);
    }

    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsNeg1_WhenGameNotFound()
    {
        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(-1);
    }


    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsNextAvailablePosition()
    {
        var user1 = CommonMethods.GetRandomGameUser(UserId1) with { UserType = UserType.Player, SeatingPosition = 0 };
        var user2 = CommonMethods.GetRandomGameUser(UserId2) with { UserType = UserType.Player, SeatingPosition = 1 };
        var user3 = CommonMethods.GetRandomGameUser(UserId3) with { UserType = UserType.Player, SeatingPosition = 2 };
        _sut.Set(_game1 with { Users = [user1, user2, user3] });

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(3);
    }

    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsMaxPlusOne_WithNonContiguousPositions()
    {
        var user1 = CommonMethods.GetRandomGameUser(UserId1) with { UserType = UserType.Player, SeatingPosition = 0 };
        var user2 = CommonMethods.GetRandomGameUser(UserId2) with { UserType = UserType.Player, SeatingPosition = 2 };
        var user3 = CommonMethods.GetRandomGameUser(UserId3) with { UserType = UserType.Player, SeatingPosition = 5 };
        _sut.Set(_game1 with { Users = [user1, user2, user3] });

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(6, "method returns max position + 1, not first gap");
    }

    #endregion

    #region SetUserRole

    [TestMethod]
    public void SetUserRole_UpdatesExpectedUsersOfChange()
    {
        Role role = Role.Gunslinger();

        var targetUser = CommonMethods.GetRandomGameUser(UserId1) with { UserType = UserType.Player };
        var storyTeller = CommonMethods.GetRandomGameUser(UserId2) with { UserType = UserType.StoryTeller };
        var spectator = CommonMethods.GetRandomGameUser(UserId3) with { UserType = UserType.Spectator };
        var otherPlayer = CommonMethods.GetRandomGameUser(UserId4) with { UserType = UserType.Player };

        _sut.Set(_game1 with { UserId = UserId1, Users = [targetUser, storyTeller, spectator, otherPlayer] });
        _sut.Set(_game1 with { UserId = UserId2, Users = [targetUser, storyTeller, spectator, otherPlayer] });
        _sut.Set(_game1 with { UserId = UserId3, Users = [targetUser, storyTeller, spectator, otherPlayer] });
        _sut.Set(_game1 with { UserId = UserId4, Users = [targetUser, storyTeller, spectator, otherPlayer] });
        _sut.Set(_game2 with { UserId = UserId1, Users = [targetUser, storyTeller, spectator, otherPlayer] });

        _sut.SetUserRole(GameId1, targetUser.Id, role);

        _sut.Get(GameId1, UserId1)!.Users.First(o => o.Id == targetUser.Id).Role.Should().BeEquivalentTo(role);
        _sut.Get(GameId1, UserId2)!.Users.First(o => o.Id == targetUser.Id).Role.Should().BeEquivalentTo(role);
        _sut.Get(GameId1, UserId3)!.Users.First(o => o.Id == targetUser.Id).Role.Should().BeEquivalentTo(role);
        _sut.Get(GameId1, UserId4)!.Users.First(o => o.Id == targetUser.Id).Role.Should().NotBeEquivalentTo(role);
        _sut.Get(GameId2, UserId1)!.Users.First(o => o.Id == targetUser.Id).Role.Should().NotBeEquivalentTo(role);
    }

    #endregion

    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
}