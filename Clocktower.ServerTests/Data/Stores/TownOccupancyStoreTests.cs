using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;

namespace Clocktower.ServerTests.Data.Stores;

[TestClass]
public class TownOccupancyStoreTests
{
    private ITownOccupancyStore _sut = null!;
    private TownOccupants _dummyOccupants = null!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new TownOccupancyStore();
        _dummyOccupants = GetDummyTownOccupants();
    }

    private static TownOccupants GetDummyTownOccupants()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var townOccupants = new TownOccupants(channelCategories);
        return townOccupants;
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
        var result = _sut.Set("guild1", _dummyOccupants);

        result.Should().BeTrue();
        _sut.Get("guild1").Should().Be(_dummyOccupants);
    }

    [TestMethod]
    public void Set_WhenGuildAlreadyExists_ReturnsFalse()
    {
        _sut.Set("guild1", _dummyOccupants);

        var result = _sut.Set("guild1", new TownOccupants([]));

        result.Should().BeFalse();
        _sut.Get("guild1").Should().BeEquivalentTo(_dummyOccupants);
    }

    [TestMethod]
    public void Get_ReturnsNull_WhenGuildDoesNotExist()
    {
        var result = _sut.Get("nonexistent");

        result.Should().BeNull();
    }

    [TestMethod]
    public void Get_ReturnsNull_WhenNoGuildId()
    {
        var result = _sut.Get((string?)null);

        result.Should().BeNull();
    }

    [TestMethod]
    public void TryUpdate_WhenGuildExists_UpdatesAndReturnsTrue()
    {
        _sut.Set("guild1", _dummyOccupants);
        var newOccupants = new TownOccupants([]);

        var result = _sut.TryUpdate("guild1", _ => newOccupants);

        result.Should().BeTrue();
        _sut.Get("guild1").Should().Be(newOccupants);
    }

    [TestMethod]
    public void TryUpdate_WhenGuildDoesNotExist_ReturnsFalse()
    {
        var result = _sut.TryUpdate("nonexistent", _ => _dummyOccupants);

        result.Should().BeFalse();
    }

    [TestMethod]
    public void Remove_WhenGuildExists_RemovesAndReturnsTrue()
    {
        _sut.Set("guild1", _dummyOccupants);

        var result = _sut.Remove("guild1");

        result.Should().BeTrue();
        _sut.Get("guild1").Should().BeNull();
    }

    [TestMethod]
    public void UlongOverloads_WorkCorrectly()
    {
        const ulong guildId = 123456789UL;

        _sut.Set(guildId, _dummyOccupants).Should().BeTrue();
        _sut.Get((ulong?)null).Should().BeNull();
        _sut.Get(guildId).Should().Be(_dummyOccupants);
        _sut.TryUpdate(guildId, _ => _dummyOccupants);
        _sut.Remove(guildId).Should().BeTrue();
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        _sut.Set("guild1", _dummyOccupants);
        _sut.Set("guild2", _dummyOccupants);

        _sut.Clear();

        _sut.Get("guild1").Should().BeNull();
        _sut.Get("guild2").Should().BeNull();
    }
    
    [TestMethod]
    public void GetTownByUser_UserNotInTown_ReturnsNull()
    {
        var store = new TownOccupancyStore();
        const string userId = "user123";
        const string guildId = "guild456";
        var townOccupants = GetDummyTownOccupants();
        store.Set(guildId, townOccupants);

        var result = store.GetTownByUser(userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetTownByUser_UserExistsInTown_ReturnsTownOccupants()
    {
        var store = new TownOccupancyStore();
        const string userId = "user123";
        const string guildId = "guild456";
        var townOccupants = CreateTownWithUser(userId);
        store.Set(guildId, townOccupants);

        var result = store.GetTownByUser(userId);

        result.Should().NotBeNull();
        result.Should().Be(townOccupants);
    }

    [TestMethod]
    public void GetTownByUser_UserInMultipleTowns_ReturnsFirstMatch()
    {
        var store = new TownOccupancyStore();
        const string userId = "user123";
        var town1 = CreateTownWithUser(userId);
        var town2 = CreateTownWithUser(userId);
        store.Set("guild1", town1);
        store.Set("guild2", town2);
        
        var result = store.GetTownByUser(userId);
        
        result.Should().NotBeNull();
        result.Should().BeOneOf(town1, town2);
    }

    private static TownOccupants CreateTownWithUser(string userId)
    {
        return new TownOccupants([
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