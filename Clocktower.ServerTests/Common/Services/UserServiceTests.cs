using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class UserServiceTests
{
    private Mock<IDiscordTownStore> _mockDiscordTownStore = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private IUserService _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordTownStore = StrictMockFactory.Create<IDiscordTownStore>();
        _mockGameStateStore = StrictMockFactory.Create<IGameStateStore>();
        _mockDiscordTownManager = StrictMockFactory.Create<IDiscordTownManager>();

        _sut = new UserService(_mockDiscordTownStore.Object, _mockGameStateStore.Object, _mockDiscordTownManager.Object);
    }

    #region GetUserName

    private void Setup_GetUserName(string userId, string? name)
    {
        TownUser? townUser = null;
        if (name != null)
        {
            townUser = CommonMethods.GetRandomTownUser(userId, name);
        }

        _mockDiscordTownManager.Setup(o => o.GetTownUser(userId)).Returns(townUser);
    }

    [TestMethod]
    [DataRow("name")]
    [DataRow("")]
    public void GetUserName_CallsGetTownUser_ReturnsName(string name)
    {
        const string userId = "123";
        Setup_GetUserName(userId, name);

        var result = _sut.GetUserName(userId);

        result.Should().Be(name);
    }

    [TestMethod]
    public void GetUserName_CallsGetTownUser_ReturnsNull_WhenNoUserFound()
    {
        const string userId = "123";
        Setup_GetUserName(userId, null);

        var result = _sut.GetUserName(userId);

        result.Should().BeNull();
    }

    #endregion

    #region UpdateGameUser

    private void SetUp_UpdateGameUser(string gameId, string userId, UserType userType, bool? isPlaying, bool updateReturn)
    {
        _mockGameStateStore.Setup(o => o.UpdateUser(gameId, userId, userType, isPlaying)).Returns(updateReturn);
    }

    [TestMethod]
    public void UpdateGameUser_PassesParametersToStore_AndReturnsStoreResult()
    {
        const string gameId = "test-game";
        const string userId = "123456";
        const UserType userType = UserType.Player;
        const bool isPlaying = true;
        const bool updateReturn = false;
        SetUp_UpdateGameUser(gameId, userId, userType, isPlaying, updateReturn);

        var result = _sut.UpdateGameUser(gameId, userId, userType, isPlaying);

        result.Should().Be(updateReturn);
    }

    #endregion


    #region UpdateDiscordPresence

    private void SetUp_UpdateDiscordPresence(string userId, string guildId, bool isPresent, VoiceState voiceState, bool updateReturn)
    {
        _mockDiscordTownManager.Setup(o => o.UpdateUserStatus(guildId, userId, isPresent, voiceState)).Returns(updateReturn);
    }

    [TestMethod]
    public void UpdateDiscordPresence_PassesParametersToStore_AndReturnsStoreResult()
    {
        const string userId = "123456";
        const string guildId = "987654";
        const bool isPresent = true;
        var voiceState = new VoiceState(true, false, true, false);
        const bool updateReturn = true;
        SetUp_UpdateDiscordPresence(userId, guildId, isPresent, voiceState, updateReturn);

        var result = _sut.UpdateDiscordPresence(userId, guildId, isPresent, voiceState);

        result.Should().Be(updateReturn);
    }

    #endregion

    #region GetTownUsersForGameUsers

    [TestMethod]
    public void GetTownUsersForGameUsers_ReturnsEmpty_WhendiscordTownNotFound()
    {
        var gameUsers = new[] { new GameUser("user1") };
        const string guildId = "guild123";

        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns((DiscordTown?)null);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_ReturnsEmpty_WhenNoGameUsersProvided()
    {
        var gameUsers = Array.Empty<GameUser>();
        const string guildId = "guild123";
        var discordTown = CreateDiscordTown();

        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns(discordTown);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_ReturnsMatchingUsers_WhenUsersFound()
    {
        var gameUsers = new[]
        {
            new GameUser("user1"),
            new GameUser("user2")
        };
        const string guildId = "guild123";

        var townUser1 = new TownUser("user1", "User One", "Avatar1");
        var townUser2 = new TownUser("user2", "User Two", "Avatar2");
        var townUser3 = new TownUser("user3", "User Three", "Avatar3");

        var discordTown = CreateDiscordTown(townUser1, townUser2, townUser3);
        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns(discordTown);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId).ToArray();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == "user1");
        result.Should().Contain(u => u.Id == "user2");
        result.Should().NotContain(u => u.Id == "user3");
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_AppliesFilter_WhenFilterProvided()
    {
        var gameUsers = new[]
        {
            new GameUser("user1"),
            new GameUser("user2")
        };
        const string guildId = "guild123";

        var townUser1 = new TownUser("user1", "User One", "Avatar1") { IsPresent = true };
        var townUser2 = new TownUser("user2", "User Two", "Avatar2") { IsPresent = false };

        var discordTown = CreateDiscordTown(townUser1, townUser2);
        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns(discordTown);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId, u => u.IsPresent).ToArray();

        result.Should().HaveCount(1);
        result.Should().Contain(u => u.Id == "user1");
        result.Should().NotContain(u => u.Id == "user2");
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_ReturnsEmpty_WhenFilterExcludesAllUsers()
    {
        var gameUsers = new[] { new GameUser("user1") };
        const string guildId = "guild123";

        var townUser1 = new TownUser("user1", "User One", "Avatar1") { IsPresent = false };
        var discordTown = CreateDiscordTown(townUser1);
        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns(discordTown);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId, u => u.IsPresent);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_ReturnsUsersFromMultipleChannels()
    {
        var gameUsers = new[]
        {
            new GameUser("user1"),
            new GameUser("user2")
        };
        const string guildId = "guild123";

        var townUser1 = new TownUser("user1", "User One", "Avatar1");
        var townUser2 = new TownUser("user2", "User Two", "Avatar2");

        var channel1 = new ChannelOccupants(new MiniChannel("ch1", "Channel 1"), [townUser1]);
        var channel2 = new ChannelOccupants(new MiniChannel("ch2", "Channel 2"), [townUser2]);

        var category = new MiniCategory("cat1", "Test Category", [channel1, channel2]);
        var discordTown = new DiscordTown([category]);

        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns(discordTown);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId).ToArray();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == "user1");
        result.Should().Contain(u => u.Id == "user2");
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_ReturnsUsersFromMultipleCategories()
    {
        var gameUsers = new[]
        {
            new GameUser("user1"),
            new GameUser("user2")
        };
        const string guildId = "guild123";

        var townUser1 = new TownUser("user1", "User One", "Avatar1");
        var townUser2 = new TownUser("user2", "User Two", "Avatar2");

        var channel1 = new ChannelOccupants(new MiniChannel("ch1", "Channel 1"), [townUser1]);
        var channel2 = new ChannelOccupants(new MiniChannel("ch2", "Channel 2"), [townUser2]);

        var category1 = new MiniCategory("cat1", "Category 1", [channel1]);
        var category2 = new MiniCategory("cat2", "Category 2", [channel2]);

        var discordTown = new DiscordTown([category1, category2]);

        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns(discordTown);

        var result = _sut.GetTownUsersForGameUsers(gameUsers, guildId).ToArray();

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == "user1");
        result.Should().Contain(u => u.Id == "user2");
    }

    [TestMethod]
    public void GetTownUsersForGameUsers_CallsStoreWithCorrectGuildId()
    {
        var gameUsers = new[] { new GameUser("user1") };
        const string guildId = "specific-guild-123";

        _mockDiscordTownStore.Setup(x => x.Get(guildId)).Returns((DiscordTown?)null);

        _ = _sut.GetTownUsersForGameUsers(gameUsers, guildId).ToList();

        _mockDiscordTownStore.Verify(x => x.Get(guildId), Times.Once);
    }

    private static DiscordTown CreateDiscordTown(params TownUser[] users)
    {
        var channel = new ChannelOccupants(
            new MiniChannel("channel1", "Test Channel"),
            users);

        var category = new MiniCategory("category1", "Test Category", [channel]);

        return new DiscordTown([category]);
    }

    #endregion
}