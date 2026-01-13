using Clocktower.Server.Common.Services;
using Clocktower.Server.Common.UpdateModels;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class GamePerspectiveServiceTests
{
    private const string GameId1 = "_game1";
    private const string GameId2 = "_game2";
    private const string GameId3 = "_game3";
    private const string UserId1 = "123";
    private const string UserId2 = "456";
    private const string UserId3 = "789";
    private const string UserId4 = "987";
    private const string GuildId = "123456789";
    private IGamePerspectiveService _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        var store = new GamePerspectiveStore();
        _sut = new GamePerspectiveService(store);
    }

    [TestMethod]
    public void GameExists_ReturnsFalse_WhenGameNotFound()
    {
        var result = _sut.GameExists(GameId1);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void GameExists_ReturnsTrue_WhenGameFound()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.GameExists(GameId1);

        result.Should().BeTrue();
    }

    #region GetFirstPerspective

    [TestMethod]
    public void GetFirstPerspective_ReturnsNull_WhenGameDoesNotExist()
    {
        var result = _sut.GetFirstPerspective(GameId1);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetFirstPerspective_ReturnsFirstPerspective_WhenGameHasSinglePerspective()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.GetFirstPerspective(GameId1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(GameId1);
    }

    [TestMethod]
    public void GetFirstPerspective_ReturnsOnePerspective_WhenGameHasMultiplePerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        var result = _sut.GetFirstPerspective(GameId1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(GameId1);
    }

    [TestMethod]
    public void GetFirstPerspective_ReturnsOnlyPerspectiveFromRequestedGame()
    {
        var user1 = new GameUser(UserId1) { UserType = UserType.Player };
        var user2 = new GameUser(UserId2) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, user1);
        _sut.InitializeGame(GameId2, GuildId, user2);

        var result = _sut.GetFirstPerspective(GameId1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(GameId1);
    }

    #endregion

    #region GetPerspective

    [TestMethod]
    public void GetPerspective_ReturnsPersonalPerspective_WhenUserHasOwnPerspective()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.GetPerspective(GameId1, UserId1);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(UserId1);
    }

    [TestMethod]
    public void GetPerspective_ReturnsOmniscientPerspective_WhenStoryTellerAccessesGame()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.GetPerspective(GameId1, UserId1);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
    }

    [TestMethod]
    public void GetPerspective_ReturnsOmniscientPerspective_WhenSpectatorAccessesGame()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var spectator = new GameUser(UserId2) { UserType = UserType.Spectator };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, spectator);

        var result = _sut.GetPerspective(GameId1, UserId2);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
    }

    [TestMethod]
    public void GetPerspective_ReturnsNull_WhenNoPersonalPerspectiveExists()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.GetPerspective(GameId1, UserId2);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetPerspective_ReturnsNull_WhenNoOmniscientPerspectiveExists()
    {
        var result = _sut.GetPerspective(GameId1, UserId1);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetPerspective_ReturnsNull_WhenUserNotInOmniscientPerspective()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.GetPerspective(GameId1, UserId2);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetPerspective_ReturnsNull_WhenUserInOmniscientPerspectiveButNotOmniscient()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player = new GameUser(UserId2) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player);

        var result = _sut.GetPerspective(GameId1, UserId2);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(UserId2);
    }

    #endregion

    #region RemoveUserFromGame

    [TestMethod]
    public void RemoveUserFromGame_DoesNothing_WhenGameDoesNotExist()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        _sut.RemoveUserFromGame(GameId2, UserId1);

        var result = _sut.GetFirstPerspective(GameId1);
        result.Should().NotBeNull();
        result!.Users.Should().ContainSingle(u => u.Id == UserId1);
    }

    [TestMethod]
    public void RemoveUserFromGame_RemovesUserPerspective()
    {
        var player = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, player);

        _sut.RemoveUserFromGame(GameId1, UserId1);

        var result = _sut.GetPerspective(GameId1, UserId1);
        result.Should().BeNull();
    }

    [TestMethod]
    public void RemoveUserFromGame_RemovesUserFromAllPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, SeatingPosition = 0 };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player, SeatingPosition = 1 };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        _sut.RemoveUserFromGame(GameId1, UserId2);

        var stPerspective = _sut.GetPerspective(GameId1, UserId1);
        stPerspective!.Users.Should().NotContain(u => u.Id == UserId2);
        stPerspective.Users.Should().HaveCount(2);

        var player2Perspective = _sut.GetPerspective(GameId1, UserId3);
        player2Perspective!.Users.Should().NotContain(u => u.Id == UserId2);
    }

    [TestMethod]
    public void RemoveUserFromGame_ReordersSeatingPositions()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, SeatingPosition = 0, IsPlaying = true };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player, SeatingPosition = 1, IsPlaying = true };
        var player3 = new GameUser(UserId4) { UserType = UserType.Player, SeatingPosition = 2, IsPlaying = true };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);
        _sut.AddUserToGame(GameId1, player3);

        _sut.RemoveUserFromGame(GameId1, UserId3);

        var perspective = _sut.GetFirstPerspective(GameId1);
        perspective!.Users.First(u => u.Id == UserId2).SeatingPosition.Should().Be(0);
        perspective.Users.First(u => u.Id == UserId4).SeatingPosition.Should().Be(1);
    }

    [TestMethod]
    public void RemoveUserFromGame_StartsAtZero_AfterRemovingFirstPlayer()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, SeatingPosition = 0, IsPlaying = true };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player, SeatingPosition = 1, IsPlaying = true };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        _sut.RemoveUserFromGame(GameId1, UserId2);

        var perspective = _sut.GetFirstPerspective(GameId1);
        perspective!.Users.First(u => u.Id == UserId3).SeatingPosition.Should().Be(0);
    }

    #endregion

    #region UpdatePrivateUser

    [TestMethod]
    public void UpdatePrivateUser_ReturnsFalse_WhenUserNotFound()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePrivateUser(GameId1, UserId2, new PrivateGameUserUpdate { Role = Role.Gunslinger() });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePrivateUser_ReturnsFalse_WhenNoChangesProvided()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePrivateUser(GameId1, UserId1, new PrivateGameUserUpdate());

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePrivateUser_ReturnsFalse_WhenRoleAlreadyMatches()
    {
        var role = Role.Gunslinger();
        var user = new GameUser(UserId1) { UserType = UserType.Player, Role = role };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePrivateUser(GameId1, UserId1, new PrivateGameUserUpdate { Role = role });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePrivateUser_ReturnsTrue_WhenRoleUpdated()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);
        var newRole = Role.Gunslinger();

        var result = _sut.UpdatePrivateUser(GameId1, UserId1, new PrivateGameUserUpdate { Role = newRole });

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId1);
        perspective!.Users.First(u => u.Id == UserId1).Role.Should().BeEquivalentTo(newRole);
    }

    [TestMethod]
    public void UpdatePrivateUser_UpdatesOwnAndOmniscientPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player);
        var newRole = Role.Gunslinger();

        _sut.UpdatePrivateUser(GameId1, UserId2, new PrivateGameUserUpdate { Role = newRole });

        var stPerspective = _sut.GetPerspective(GameId1, UserId1);
        stPerspective!.Users.First(u => u.Id == UserId2).Role.Should().BeEquivalentTo(newRole);

        var playerPerspective = _sut.GetPerspective(GameId1, UserId2);
        playerPerspective!.Users.First(u => u.Id == UserId2).Role.Should().BeEquivalentTo(newRole);
    }

    [TestMethod]
    public void UpdatePrivateUser_DoesNotUpdateOtherPlayerPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);
        var newRole = Role.Gunslinger();

        _sut.UpdatePrivateUser(GameId1, UserId2, new PrivateGameUserUpdate { Role = newRole });

        var player2Perspective = _sut.GetPerspective(GameId1, UserId3);
        player2Perspective!.Users.First(u => u.Id == UserId2).Role.Should().BeNull();
    }

    #endregion

    #region AddUserToGame

    [TestMethod]
    public void AddUserToGame_ReturnsFalse_WhenGameDoesNotExist()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };

        var result = _sut.AddUserToGame(GameId1, user);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void AddUserToGame_ReturnsFalse_WhenUserAlreadyInGame()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.AddUserToGame(GameId1, user);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void AddUserToGame_AddsPlayerWithOwnPerspective()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.AddUserToGame(GameId1, player);

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId2);
        perspective.Should().NotBeNull();
        perspective!.UserId.Should().Be(UserId2);
    }

    [TestMethod]
    public void AddUserToGame_AddsOmniscientUserToSharedPerspective()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var spectator = new GameUser(UserId2) { UserType = UserType.Spectator };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.AddUserToGame(GameId1, spectator);

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId2);
        perspective!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
    }

    [TestMethod]
    public void AddUserToGame_AddsUserToAllPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);

        _sut.AddUserToGame(GameId1, player2);

        var stPerspective = _sut.GetPerspective(GameId1, UserId1);
        stPerspective!.Users.Should().Contain(u => u.Id == UserId3);

        var player1Perspective = _sut.GetPerspective(GameId1, UserId2);
        player1Perspective!.Users.Should().Contain(u => u.Id == UserId3);
    }

    [TestMethod]
    public void AddUserToGame_PlayerSeesPublicUserDataOfOtherPlayers()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, Role = Role.Gunslinger() };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);

        _sut.AddUserToGame(GameId1, player2);

        var player2Perspective = _sut.GetPerspective(GameId1, UserId3);
        var player1InPerspective = player2Perspective!.Users.First(u => u.Id == UserId2);
        player1InPerspective.Role.Should().BeNull();
    }

    [TestMethod]
    public void AddUserToGame_CreatesOmniscientPerspective_WhenFirstOmniscientUserAdded()
    {
        var player = new GameUser(UserId1) { UserType = UserType.Player };
        var storyTeller = new GameUser(UserId2) { UserType = UserType.StoryTeller };
        _sut.InitializeGame(GameId1, GuildId, player);

        var result = _sut.AddUserToGame(GameId1, storyTeller);

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId2);
        perspective.Should().NotBeNull();
        perspective!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
        perspective.Users.Should().Contain(u => u.Id == UserId2);
    }

    #endregion

    #region InitializeGame

    [TestMethod]
    public void InitializeGame_ReturnsNull_WhenGameAlreadyExists()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.InitializeGame(GameId1, GuildId, user);

        result.Should().BeNull();
    }

    [TestMethod]
    public void InitializeGame_CreatesGameWithPlayer()
    {
        var player = new GameUser(UserId1) { UserType = UserType.Player };

        var result = _sut.InitializeGame(GameId1, GuildId, player);

        result.Should().NotBeNull();
        result!.Id.Should().Be(GameId1);
        result.GuildId.Should().Be(GuildId);
        result.UserId.Should().Be(UserId1);
        result.Users.Should().ContainSingle(u => u.Id == UserId1);
    }

    [TestMethod]
    public void InitializeGame_CreatesGameWithStoryTeller_UsesOmniscientKey()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };

        var result = _sut.InitializeGame(GameId1, GuildId, storyTeller);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
    }

    [TestMethod]
    public void InitializeGame_CreatesGameWithSpectator_UsesOmniscientKey()
    {
        var spectator = new GameUser(UserId1) { UserType = UserType.Spectator };

        var result = _sut.InitializeGame(GameId1, GuildId, spectator);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
    }

    #endregion

    #region GetAllPerspectivesForGame

    [TestMethod]
    public void GetAllPerspectivesForGame_ReturnsEmpty_WhenGameDoesNotExist()
    {
        var result = _sut.GetAllPerspectivesForGame(GameId1);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAllPerspectivesForGame_ReturnsAllPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        var result = _sut.GetAllPerspectivesForGame(GameId1).ToList();

        result.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetAllPerspectivesForGame_ReturnsOnlyRequestedGame()
    {
        var user1 = new GameUser(UserId1) { UserType = UserType.Player };
        var user2 = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user1);
        _sut.InitializeGame(GameId2, GuildId, user2);

        var result = _sut.GetAllPerspectivesForGame(GameId1);

        result.Should().AllSatisfy(p => p.Id.Should().Be(GameId1));
    }

    #endregion

    #region GetUserGames

    [TestMethod]
    public void GetUserGames_ReturnsEmpty_WhenUserHasNoGames()
    {
        var result = _sut.GetUserGames(UserId1);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetUserGames_ReturnsPlayerGames()
    {
        var player = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, player);
        _sut.InitializeGame(GameId2, GuildId, player);

        var result = _sut.GetUserGames(UserId1).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == GameId1);
        result.Should().Contain(p => p.Id == GameId2);
    }

    [TestMethod]
    public void GetUserGames_ReturnsOmniscientGames()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.InitializeGame(GameId2, GuildId, storyTeller);

        var result = _sut.GetUserGames(UserId1).ToList();

        result.Should().HaveCount(2);
    }

    [TestMethod]
    public void GetUserGames_DoesNotReturnDuplicates_WhenUserInMultiplePerspectivesOfSameGame()
    {
        var player = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, player);

        var result = _sut.GetUserGames(UserId1).ToList();

        result.Should().ContainSingle();
    }

    #endregion

    #region GetGuildGameIds

    [TestMethod]
    public void GetGuildGameIds_ReturnsEmpty_WhenGuildHasNoGames()
    {
        var result = _sut.GetGuildGameIds(GuildId);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetGuildGameIds_ReturnsAllGameIdsForGuild()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);
        _sut.InitializeGame(GameId2, GuildId, user);
        _sut.InitializeGame(GameId3, "other-guild", user);

        var result = _sut.GetGuildGameIds(GuildId).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(GameId1);
        result.Should().Contain(GameId2);
        result.Should().NotContain(GameId3);
    }

    #endregion

    #region RemoveGame

    [TestMethod]
    public void RemoveGame_ReturnsFalse_WhenGameDoesNotExist()
    {
        var result = _sut.RemoveGame(GameId1);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void RemoveGame_ReturnsTrue_WhenGameRemoved()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.RemoveGame(GameId1);

        result.Should().BeTrue();
    }

    [TestMethod]
    public void RemoveGame_RemovesAllPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        _sut.RemoveGame(GameId1);

        _sut.GetPerspective(GameId1, UserId1).Should().BeNull();
        _sut.GetPerspective(GameId1, UserId2).Should().BeNull();
        _sut.GetPerspective(GameId1, UserId3).Should().BeNull();
    }

    [TestMethod]
    public void RemoveGame_DoesNotAffectOtherGames()
    {
        var user1 = new GameUser(UserId1) { UserType = UserType.Player };
        var user2 = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user1);
        _sut.InitializeGame(GameId2, GuildId, user2);

        _sut.RemoveGame(GameId1);

        _sut.GetPerspective(GameId2, UserId2).Should().NotBeNull();
    }

    #endregion

    #region SetTime

    [TestMethod]
    public void SetTime_UpdatesGameTimeForAllPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);

        _sut.SetTime(GameId1, GameTime.Night);

        var stPerspective = _sut.GetPerspective(GameId1, UserId1);
        stPerspective!.GameTime.Should().Be(GameTime.Night);

        var playerPerspective = _sut.GetPerspective(GameId1, UserId2);
        playerPerspective!.GameTime.Should().Be(GameTime.Night);
    }

    [TestMethod]
    public void SetTime_DoesNotAffectOtherGames()
    {
        var user1 = new GameUser(UserId1) { UserType = UserType.Player };
        var user2 = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user1);
        _sut.InitializeGame(GameId2, GuildId, user2);

        _sut.SetTime(GameId1, GameTime.Night);

        var game2Perspective = _sut.GetPerspective(GameId2, UserId2);
        game2Perspective!.GameTime.Should().NotBe(GameTime.Night);
    }

    #endregion

    #region SetScript

    [TestMethod]
    public void SetScript_UpdatesScriptForAllPerspectives()
    {
        var script = new Script("Name", "Author", []);
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);

        _sut.SetScript(GameId1, script);

        var stPerspective = _sut.GetPerspective(GameId1, UserId1);
        stPerspective!.Script.Should().Be(script);

        var playerPerspective = _sut.GetPerspective(GameId1, UserId2);
        playerPerspective!.Script.Should().Be(script);
    }

    [TestMethod]
    public void SetScript_DoesNotAffectOtherGames()
    {
        var script = new Script("Name", "Author", []);
        var user1 = new GameUser(UserId1) { UserType = UserType.Player };
        var user2 = new GameUser(UserId2) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user1);
        _sut.InitializeGame(GameId2, GuildId, user2);

        _sut.SetScript(GameId1, script);

        var game2Perspective = _sut.GetPerspective(GameId2, UserId2);
        game2Perspective!.Script.Should().NotBe(script);
    }

    #endregion

    #region GetAll

    [TestMethod]
    public void GetAll_ReturnsEmpty_WhenNoGamesExist()
    {
        var result = _sut.GetAll();

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetAll_ReturnsAllPerspectives()
    {
        var user1 = new GameUser(UserId1) { UserType = UserType.Player };
        var user2 = new GameUser(UserId2) { UserType = UserType.Player };
        var user3 = new GameUser(UserId3) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, user1);
        _sut.InitializeGame(GameId2, GuildId, user2);
        _sut.InitializeGame(GameId3, GuildId, user3);

        var result = _sut.GetAll().ToList();

        result.Should().HaveCount(3);
        result.Should().Contain(p => p.Id == GameId1);
        result.Should().Contain(p => p.Id == GameId2);
        result.Should().Contain(p => p.Id == GameId3);
    }

    [TestMethod]
    public void GetAll_ReturnsMultiplePerspectivesFromSameGame()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        var result = _sut.GetAll().ToList();

        result.Should().HaveCount(3);
        result.Where(p => p.Id == GameId1).Should().HaveCount(3);
    }

    #endregion

    #region UpdatePublicUser

    [TestMethod]
    public void UpdatePublicUser_ReturnsFalse_WhenUserNotFound()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePublicUser(GameId1, UserId2, new PublicGameUserUpdate { IsDead = true });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_ReturnsFalse_WhenNoChangesProvided()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate());

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_ReturnsFalse_WhenAllPropertiesAlreadyMatch()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player, IsDead = true, IsMarked = true };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate
        {
            UserType = UserType.Player,
            IsDead = true,
            IsMarked = true
        });

        result.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_ReturnsTrue_WhenSinglePropertyUpdated()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player, IsDead = false };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate { IsDead = true });

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId1);
        perspective!.Users.First(u => u.Id == UserId1).IsDead.Should().BeTrue();
    }

    [TestMethod]
    public void UpdatePublicUser_UpdatesMultipleProperties()
    {
        var user = new GameUser(UserId1) { UserType = UserType.Player, IsDead = false, IsMarked = false, HasVoteToken = true };
        _sut.InitializeGame(GameId1, GuildId, user);

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate
        {
            IsDead = true,
            IsMarked = true,
            HasVoteToken = false
        });

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId1);
        var updatedUser = perspective!.Users.First(u => u.Id == UserId1);
        updatedUser.IsDead.Should().BeTrue();
        updatedUser.IsMarked.Should().BeTrue();
        updatedUser.HasVoteToken.Should().BeFalse();
    }

    [TestMethod]
    public void UpdatePublicUser_UpdatesAllPerspectives()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, IsDead = false };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);

        _sut.UpdatePublicUser(GameId1, UserId2, new PublicGameUserUpdate { IsDead = true });

        var stPerspective = _sut.GetPerspective(GameId1, UserId1);
        stPerspective!.Users.First(u => u.Id == UserId2).IsDead.Should().BeTrue();

        var player1Perspective = _sut.GetPerspective(GameId1, UserId2);
        player1Perspective!.Users.First(u => u.Id == UserId2).IsDead.Should().BeTrue();

        var player2Perspective = _sut.GetPerspective(GameId1, UserId3);
        player2Perspective!.Users.First(u => u.Id == UserId2).IsDead.Should().BeTrue();
    }

    [TestMethod]
    public void UpdatePublicUser_OnlyUpdatesSpecifiedProperties()
    {
        var user = new GameUser(UserId1)
        {
            UserType = UserType.Player,
            IsDead = false,
            IsMarked = false,
            SeatingPosition = 5
        };
        _sut.InitializeGame(GameId1, GuildId, user);

        _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate { IsDead = true });

        var perspective = _sut.GetPerspective(GameId1, UserId1);
        var updatedUser = perspective!.Users.First(u => u.Id == UserId1);
        updatedUser.IsDead.Should().BeTrue();
        updatedUser.IsMarked.Should().BeFalse();
        updatedUser.SeatingPosition.Should().Be(5);
    }

    [TestMethod]
    public void UpdatePublicUser_HandlesUserTypeTransition_PlayerToStoryTeller()
    {
        var player = new GameUser(UserId1) { UserType = UserType.Player };
        _sut.InitializeGame(GameId1, GuildId, player);

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate { UserType = UserType.StoryTeller });

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId1);
        perspective!.UserId.Should().Be(IGamePerspectiveStore.OmniscientKey);
    }

    [TestMethod]
    public void UpdatePublicUser_HandlesUserTypeTransition_StoryTellerToPlayer()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.UpdatePublicUser(GameId1, UserId1, new PublicGameUserUpdate { UserType = UserType.Player });

        result.Should().BeTrue();
        var perspective = _sut.GetPerspective(GameId1, UserId1);
        perspective!.UserId.Should().Be(UserId1);
    }

    [TestMethod]
    public void UpdatePublicUser_DoesNothing_WhenUserNotInGame()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.UpdatePublicUser(GameId1, UserId2, new PublicGameUserUpdate { UserType = UserType.Player });

        result.Should().BeFalse();
    }

    #endregion

    #region GetNextAvailableSeatingPosition

    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsNegativeOne_WhenGameDoesNotExist()
    {
        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(-1);
    }

    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsZero_WhenNoPlayersExist()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        _sut.InitializeGame(GameId1, GuildId, storyTeller);

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(0);
    }

    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsNextPosition_WhenPlayersExist()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, SeatingPosition = 0 };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player, SeatingPosition = 1 };
        var player3 = new GameUser(UserId4) { UserType = UserType.Player, SeatingPosition = 2 };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);
        _sut.AddUserToGame(GameId1, player3);

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(3);
    }

    [TestMethod]
    public void GetNextAvailableSeatingPosition_ReturnsMaxPlusOne_WithNonContiguousPositions()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        var player1 = new GameUser(UserId2) { UserType = UserType.Player, SeatingPosition = 0 };
        var player2 = new GameUser(UserId3) { UserType = UserType.Player, SeatingPosition = 5 };
        var player3 = new GameUser(UserId4) { UserType = UserType.Player, SeatingPosition = 2 };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, player1);
        _sut.AddUserToGame(GameId1, player2);
        _sut.AddUserToGame(GameId1, player3);

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(6);
    }

    [TestMethod]
    public void GetNextAvailableSeatingPosition_IgnoresNonPlayers()
    {
        var storyTeller = new GameUser(UserId1) { UserType = UserType.StoryTeller, SeatingPosition = -1 };
        var spectator = new GameUser(UserId2) { UserType = UserType.Spectator, SeatingPosition = -1 };
        var player = new GameUser(UserId3) { UserType = UserType.Player, SeatingPosition = 0 };

        _sut.InitializeGame(GameId1, GuildId, storyTeller);
        _sut.AddUserToGame(GameId1, spectator);
        _sut.AddUserToGame(GameId1, player);

        var result = _sut.GetNextAvailableSeatingPosition(GameId1);

        result.Should().Be(1);
    }

    #endregion
}