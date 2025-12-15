using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;

namespace Clocktower.ServerTests.Data.Stores;

[TestClass]
public class DiscordTownStoreTests
{
    private IDiscordTownStore _sut = null!;
    private DiscordTown _dummy = null!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new DiscordTownStore();
        _dummy = GetDummyDiscordTown();
    }

    private static DiscordTown GetDummyDiscordTown()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var discordTown = new DiscordTown(channelCategories);
        return discordTown;
    }

    private static readonly MiniCategory DayCategory = new("1001", "Day Category", [
        new ChannelOccupants(new MiniChannel("2001", "Day Channel 1"), [
            new TownUser("3001", "User3001", string.Empty)
        ]),
        new ChannelOccupants(new MiniChannel("2002", "Day Channel 2"), [
            new TownUser("3002", "User3002", string.Empty),
            new TownUser("3003", "User3003", string.Empty)
        ])
    ]);

    private static readonly MiniCategory NightCategory = new("1002", "Night Category", [
        new ChannelOccupants(new MiniChannel("2203", "Night Channel 1"), []),
        new ChannelOccupants(new MiniChannel("2204", "Night Channel 2"), [
            new TownUser("3004", "User3004", string.Empty)
        ]),
        new ChannelOccupants(new MiniChannel("2205", "Night Channel 3"), []),
    ]);

    [TestMethod]
    public void Set_WhenGuildDoesNotExist_ReturnsTrue()
    {
        var result = _sut.Set("1", _dummy);

        result.Should().BeTrue();
        _sut.Get("1").Should().Be(_dummy);
    }

    [TestMethod]
    public void Set_WhenGuildAlreadyExists_ReturnsFalse()
    {
        _sut.Set("1", _dummy);

        var result = _sut.Set("1", new DiscordTown([]));

        result.Should().BeFalse();
        _sut.Get("1").Should().BeEquivalentTo(_dummy);
    }

    [TestMethod]
    public void Get_ReturnsNull_WhenGuildDoesNotExist()
    {
        var result = _sut.Get("9999");

        result.Should().BeNull();
    }

    [TestMethod]
    public void Get_ReturnsNull_WhenNoGuildId()
    {
        var result = _sut.Get(null);

        result.Should().BeNull();
    }

    [TestMethod]
    public void TryUpdate_WhenGuildExists_UpdatesAndReturnsTrue()
    {
        _sut.Set("1", _dummy);
        var newOccupants = new DiscordTown([]);

        var result = _sut.TryUpdate("1", _ => newOccupants);

        result.Should().BeTrue();
        _sut.Get("1").Should().Be(newOccupants);
    }

    [TestMethod]
    public void TryUpdate_WhenGuildDoesNotExist_ReturnsFalse()
    {
        var result = _sut.TryUpdate("9999", _ => _dummy);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void Remove_WhenGuildExists_RemovesAndReturnsTrue()
    {
        _sut.Set("1", _dummy);

        var result = _sut.Remove("1");

        result.Should().BeTrue();
        _sut.Get("1").Should().BeNull();
    }
    
    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        _sut.Set("1", _dummy);
        _sut.Set("2", _dummy);

        _sut.Clear();

        _sut.Get("1").Should().BeNull();
        _sut.Get("2").Should().BeNull();
    }

    [TestMethod]
    public void GetTownByUser_UserNotInTown_ReturnsNull()
    {
        var store = new DiscordTownStore();
        const string userId = "123";
        const string guildId = "456";
        var discordTown = GetDummyDiscordTown();
        store.Set(guildId, discordTown);

        var result = store.GetTownByUser(userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetTownByUser_UserExistsInTown_ReturnsDiscordTown()
    {
        var store = new DiscordTownStore();
        const string userId = "123";
        const string guildId = "456";
        var discordTown = CreateTownWithUser(userId);
        store.Set(guildId, discordTown);

        var result = store.GetTownByUser(userId);

        result.Should().NotBeNull();
        result.Should().Be(discordTown);
    }

    [TestMethod]
    public void GetTownByUser_UserInMultipleTowns_ReturnsFirstMatch()
    {
        var store = new DiscordTownStore();
        const string userId = "123";
        var town1 = CreateTownWithUser(userId);
        var town2 = CreateTownWithUser(userId);
        store.Set("1", town1);
        store.Set("2", town2);

        var result = store.GetTownByUser(userId);

        result.Should().NotBeNull();
        result.Should().BeOneOf(town1, town2);
    }

    private static DiscordTown CreateTownWithUser(string userId)
    {
        return new DiscordTown([
            new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [
                new ChannelOccupants(new MiniChannel(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString()),
                    [
                        new TownUser(userId, "User", "Avatar")
                    ]
                )
            ])
        ]);
    }
}