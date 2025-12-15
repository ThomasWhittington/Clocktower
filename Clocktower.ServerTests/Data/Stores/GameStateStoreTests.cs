using Clocktower.Server.Data;
using Clocktower.Server.Data.Extensions;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Data.Stores;

[TestClass]
public class GameStateStoreTests
{
    private const string UserId = "123";
    private IGameStateStore _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        _sut = new GameStateStore();
    }

    private static GameState NewGame(string id)
    {
        return new GameState { Id = id, GuildId = CommonMethods.GetRandomSnowflakeStringId() };
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
        var game1 = NewGame("game1");
        _sut.Set(game1);

        var result = _sut.GameExists(game1.Id);

        result.Should().BeTrue();
    }


    [TestMethod]
    public void Set_WhenGameDoesNotExist_ReturnsTrue()
    {
        var game1 = NewGame("game1");
        var result = _sut.Set(game1);

        result.Should().BeTrue();
        _sut.Get("game1").Should().Be(game1);
    }

    [TestMethod]
    public void Set_WhenGameAlreadyExists_ReturnsFalse()
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);

        var result = _sut.Set(NewGame("game1"));

        result.Should().BeFalse();
        _sut.Get("game1").Should().BeEquivalentTo(game1);
    }

    [TestMethod]
    public void Get_WhenGameDoesNotExist_ReturnsNull()
    {
        var result = _sut.Get("nonexistent");

        result.Should().BeNull();
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        var game1 = NewGame("game1");
        var game2 = NewGame("game2");
        _sut.Set(game1);
        _sut.Set(game2);

        _sut.Clear();

        _sut.Get("game1").Should().BeNull();
        _sut.Get("game2").Should().BeNull();
    }

    [TestMethod]
    public void Remove_WhenGameExists_RemovesAndReturnsTrue()
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);

        var result = _sut.Remove("game1");

        result.Should().BeTrue();
        _sut.Get("game1").Should().BeNull();
    }

    [TestMethod]
    public void TryUpdate_WhenGameExists_UpdatesAndReturnsTrue()
    {
        var game1 = NewGame("game1");
        var game2 = NewGame("game2");
        _sut.Set(game1);

        var result = _sut.TryUpdate("game1", _ => game2);

        result.Should().BeTrue();
        _sut.Get("game1").Should().Be(game2);
    }

    [TestMethod]
    public void TryUpdate_WhenGameDoesNotExist_ReturnsFalse()
    {
        var result = _sut.TryUpdate("nonexistent", null!);

        result.Should().BeFalse();
    }

    [TestMethod]
    [DynamicData(nameof(GetGameTimeValues))]
    public void SetTime_UpdatesGameTime(GameTime gameTime)
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);

        _sut.SetTime("game1", gameTime);

        var output = _sut.Get("game1");
        output.Should().NotBeNull();
        output.GameTime.Should().Be(gameTime);
    }

    [TestMethod]
    public void AddUserToGame_AddsUser()
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(UserId);

        _sut.AddUserToGame("game1", user);

        var output = _sut.Get("game1");
        output.Should().NotBeNull();
        output.Users.Should().Contain(o => o.Id == UserId);
    }

    [TestMethod]
    public void GetAll_ReturnsAll()
    {
        var game1 = NewGame("game1");
        var game2 = NewGame("game2");
        var game3 = NewGame("game3");
        _sut.Set(game1);
        _sut.Set(game2);
        _sut.Set(game3);

        var result = _sut.GetAll().ToList();

        result.Should().Contain(o => o.Id == "game1");
        result.Should().Contain(o => o.Id == "game2");
        result.Should().Contain(o => o.Id == "game3");
    }

    [TestMethod]
    public void GetGuildGames_ReturnsGuildGames()
    {
        const string guildId = "123456789";
        var game1 = NewGame("game1");
        var game2 = NewGame("game2");
        var game3 = NewGame("game3");
        game2.GuildId = guildId;
        game3.GuildId = guildId;
        _sut.Set(game1);
        _sut.Set(game2);
        _sut.Set(game3);

        var result = _sut.GetGuildGames(guildId).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == "game2");
        result.Should().Contain(o => o.Id == "game3");
    }

    [TestMethod]
    public void GetUserGames_ReturnsUserGames()
    {
        var game1 = NewGame("game1");
        var game2 = NewGame("game2");
        var game3 = NewGame("game3");
        game1.Users.Add(CommonMethods.GetRandomGameUser());
        game2.Users.Add(CommonMethods.GetRandomGameUser(UserId));
        game2.Users.Add(CommonMethods.GetRandomGameUser());
        game3.Users.Add(CommonMethods.GetRandomGameUser(UserId));
        _sut.Set(game1);
        _sut.Set(game2);
        _sut.Set(game3);

        var result = _sut.GetUserGames(UserId).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == "game2");
        result.Should().Contain(o => o.Id == "game3");
    }

    #region UpdateUser

    [TestMethod]
    public void UpdateUser_DoesNotChange_WhenNoUserFound()
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);

        var result = _sut.UpdateUser(game1.Id, UserId);
        var val = _sut.Get(game1.Id);
        result.Should().BeTrue();
        val.Should().NotBeNull();
        val.Should().BeEquivalentTo(game1);
    }

    [TestMethod]
    public void UpdateUser_ReturnsOriginal_NoChangesRequested()
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(UserId);
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, UserId);
        var val = _sut.Get(game1.Id);
        result.Should().BeTrue();
        val.Should().NotBeNull();
        val.Should().BeEquivalentTo(game1);
    }

    [TestMethod]
    [DynamicData(nameof(GetUserTypeValues))]
    public void UpdateUser_Updates_UserType(UserType userType)
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(UserId);
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, UserId, userType: userType);

        var val = _sut.Get(game1.Id);
        result.Should().BeTrue();
        val.Should().NotBeNull();
        var thisUser = val.GetUser(UserId);
        thisUser.Should().NotBeNull();
        thisUser.UserType.Should().Be(userType);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void UpdateUser_Updates_IsPlaying(bool isPlaying)
    {
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(UserId);
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, UserId, isPlaying: isPlaying);

        var val = _sut.Get(game1.Id);
        result.Should().BeTrue();
        val.Should().NotBeNull();
        var thisUser = val.GetUser(UserId);
        thisUser.Should().NotBeNull();
        thisUser.IsPlaying.Should().Be(isPlaying);
    }

    #endregion


    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
    private static IEnumerable<object[]> GetUserTypeValues() => TestDataProvider.GetAllEnumValues<UserType>();
}