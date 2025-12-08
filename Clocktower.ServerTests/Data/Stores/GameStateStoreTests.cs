using Clocktower.Server.Data;
using Clocktower.Server.Data.Extensions;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Data.Stores;

[TestClass]
public class GameStateStoreTests
{
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
        const string userId = "user-id";
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(userId);

        _sut.AddUserToGame("game1", user);

        var output = _sut.Get("game1");
        output.Should().NotBeNull();
        output.Users.Should().Contain(o => o.Id == userId);
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
        const string userId = "user-id";
        var game1 = NewGame("game1");
        var game2 = NewGame("game2");
        var game3 = NewGame("game3");
        game1.Users.Add(CommonMethods.GetRandomGameUser());
        game2.Users.Add(CommonMethods.GetRandomGameUser(userId));
        game2.Users.Add(CommonMethods.GetRandomGameUser());
        game3.Users.Add(CommonMethods.GetRandomGameUser(userId));
        _sut.Set(game1);
        _sut.Set(game2);
        _sut.Set(game3);

        var result = _sut.GetUserGames(userId).ToList();

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == "game2");
        result.Should().Contain(o => o.Id == "game3");
    }

    [TestMethod]
    public void UlongOverloads_WorkCorrectly()
    {
        const ulong guildId = 123456789UL;
        var game1 = NewGame("game1");
        game1.GuildId = guildId.ToString();
        _sut.Set(game1).Should().BeTrue();

        _sut.GetGuildGames(guildId).Should().Contain(game1);
    }

    #region UpdateUser

    [TestMethod]
    public void UpdateUser_ReturnsOriginal_WhenNoUserFound()
    {
        const ulong userId = 1L;
        var game1 = NewGame("game1");
        _sut.Set(game1);

        var result = _sut.UpdateUser(game1.Id, userId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(game1);
    }

    [TestMethod]
    public void UpdateUser_ReturnsOriginal_NoChangesRequested()
    {
        const ulong userId = 1L;
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(userId.ToString());
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, userId);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(game1);
    }

    [TestMethod]
    [DynamicData(nameof(GetUserTypeValues))]
    public void UpdateUser_Updates_UserType(UserType userType)
    {
        const ulong userId = 1L;
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(userId.ToString());
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, userId, userType: userType);
        result.Should().NotBeNull();
        var thisUser = result.GetUser(userId.ToString());
        thisUser.Should().NotBeNull();
        thisUser.UserType.Should().Be(userType);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void UpdateUser_Updates_IsPlaying(bool isPlaying)
    {
        const ulong userId = 1L;
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(userId.ToString());
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, userId, isPlaying: isPlaying);
        result.Should().NotBeNull();
        var thisUser = result.GetUser(userId.ToString());
        thisUser.Should().NotBeNull();
        thisUser.IsPlaying.Should().Be(isPlaying);
    }
    
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void UpdateUser_Updates_IsPresent(bool isPresent)
    {
        const ulong userId = 1L;
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(userId.ToString());
        _sut.AddUserToGame("game1", user);

        var result = _sut.UpdateUser(game1.Id, userId, isPresent: isPresent);
        result.Should().NotBeNull();
        var thisUser = result.GetUser(userId.ToString());
        thisUser.Should().NotBeNull();
        thisUser.IsPresent.Should().Be(isPresent);
    }
    
    [TestMethod]
    [DynamicData(nameof(GetVoiceStateCombinations))]
    public void UpdateUser_Updates_VoiceState(bool isServerMuted, bool isSelfMuted, bool isServerDeafened, bool isSelfDeafened)
    {
        const ulong userId = 1L;
        var game1 = NewGame("game1");
        _sut.Set(game1);
        var user = CommonMethods.GetRandomGameUser(userId.ToString());
        _sut.AddUserToGame("game1", user);
        var voiceState = new VoiceState(isServerMuted, isServerDeafened, isSelfMuted, isSelfDeafened);
        
        
        var result = _sut.UpdateUser(game1.Id, userId, voiceState: voiceState);
        result.Should().NotBeNull();
        var thisUser = result.GetUser(userId.ToString());
        thisUser.Should().NotBeNull();
        thisUser.VoiceState.Should().Be(voiceState);
    }
    #endregion


    private static IEnumerable<object[]> GetGameTimeValues() => TestDataProvider.GetAllEnumValues<GameTime>();
    private static IEnumerable<object[]> GetUserTypeValues() => TestDataProvider.GetAllEnumValues<UserType>();
    public static IEnumerable<object[]> GetVoiceStateCombinations() => TestDataProvider.GenerateBooleanCombinations(4);

}