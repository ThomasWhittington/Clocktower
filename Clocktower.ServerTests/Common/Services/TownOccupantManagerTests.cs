using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Wrappers;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class TownOccupantManagerTests
{
    private Mock<ITownOccupancyStore> _mockTownOccupancyStore = null!;
    private ITownOccupantManager _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockTownOccupancyStore = new Mock<ITownOccupancyStore>();
        _sut = new TownOccupantManager(_mockTownOccupancyStore.Object);
    }

    private static TownOccupants GetDummyTownOccupants()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var townOccupants = new TownOccupants(channelCategories);
        return townOccupants;
    }

    private static readonly MiniCategory DayCategory = new("1001", "Day Category", [
        new ChannelOccupants(new MiniChannel("2001", "Day Channel 1"), [
            new GameUser("3001", "User3001", string.Empty)
        ]),
        new ChannelOccupants(new MiniChannel("2002", "Day Channel 2"), [
            new GameUser("3002", "User3002", string.Empty),
            new GameUser("3003", "User3003", string.Empty)
        ])
    ]);

    private static readonly MiniCategory NightCategory = new("1002", "Night Category", [
        new ChannelOccupants(new MiniChannel("2203", "Night Channel 1"), []),
        new ChannelOccupants(new MiniChannel("2204", "Night Channel 2"), [
            new GameUser("3004", "User3004", string.Empty)
        ]),
        new ChannelOccupants(new MiniChannel("2205", "Night Channel 3"), []),
    ]);


    private static TownOccupants CreateEmptyTownOccupants() => new([]);


    #region FindUserChannel Tests

    [TestMethod]
    public void FindUserChannel_WhenUserExists_ReturnsCorrectChannel()
    {
        var occupants = GetDummyTownOccupants();
        const string userId = "3001";

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().NotBeNull();
        result.Channel.Id.Should().Be("2001");
        result.Channel.Name.Should().Be("Day Channel 1");
        result.Occupants.Should().Contain(u => u.Id == userId);
    }

    [TestMethod]
    public void FindUserChannel_WhenUserExistsInSecondChannel_ReturnsCorrectChannel()
    {
        var occupants = GetDummyTownOccupants();
        const string userId = "3003";

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().NotBeNull();
        result.Channel.Id.Should().Be("2002");
        result.Channel.Name.Should().Be("Day Channel 2");
        result.Occupants.Should().Contain(u => u.Id == userId);
    }

    [TestMethod]
    public void FindUserChannel_WhenUserDoesNotExist_ReturnsNull()
    {
        var occupants = GetDummyTownOccupants();
        const string userId = "nonexistent";

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenTownOccupantsIsEmpty_ReturnsNull()
    {
        var occupants = CreateEmptyTownOccupants();
        const string userId = "3001";

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenUserIdIsEmpty_ReturnsNull()
    {
        var occupants = GetDummyTownOccupants();
        const string userId = "";

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenUserIdIsNull_ReturnsNull()
    {
        var occupants = GetDummyTownOccupants();
        string userId = null!;

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenChannelHasMultipleUsers_ReturnsChannelWithAllUsers()
    {
        var occupants = GetDummyTownOccupants();
        const string userId = "3002";

        var result = _sut.FindUserChannel(occupants, userId);

        result.Should().NotBeNull();
        result.Channel.Id.Should().Be("2002");
        result.Occupants.Should().HaveCount(2);
        result.Occupants.Should().Contain(o => o.Id == "3002");
        result.Occupants.Should().Contain(o => o.Id == "3003");
    }

    #endregion

    #region MoveUser Tests

    [TestMethod]
    public void MoveUser_WhenUserNotInAnyChannel_AddsUserToNewChannel()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3005");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        var targetChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2001");

        targetChannel.Occupants.Should().HaveCount(2);
        targetChannel.Occupants.Should().Contain(u => u.Id == "3005");
    }

    [TestMethod]
    public void MoveUser_WhenUserInSameChannel_ReturnsUnchanged()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        result.Should().BeSameAs(occupants);
    }

    [TestMethod]
    public void MoveUser_WhenMovingBetweenChannels_UpdatesBothChannels()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3003");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        var sourceChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2002");

        sourceChannel.Occupants.Should().HaveCount(1);
        sourceChannel.Occupants.Should().NotContain(u => u.Id == "3003");

        var targetChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2001");

        targetChannel.Occupants.Should().HaveCount(2);
        targetChannel.Occupants.Should().Contain(u => u.Id == "3001");
        targetChannel.Occupants.Should().Contain(u => u.Id == "3003");
    }

    [TestMethod]
    public void MoveUser_WhenMovingToNullChannel_RemovesUserFromCurrentChannel()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3003");

        var result = _sut.MoveUser(occupants, mockUser, null);

        var sourceChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2002");
        sourceChannel.Occupants.Should().HaveCount(1);
        sourceChannel.Occupants.Should().NotContain(u => u.Id == "3003");
    }

    [TestMethod]
    public void MoveUser_WhenMovingToNewChannelInDifferentCategory_UpdatesCorrectCategories()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("2205");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        var category1 = result.ChannelCategories.Should().Contain(c => c.Id == "1001").Subject;
        var channel1 = category1.Channels.Should().Contain(ch => ch.Channel.Id == "2001").Subject;
        channel1.Occupants.Should().HaveCount(0);
        channel1.Occupants.Should().NotContain(u => u.Id == "3001");

        var category2 = result.ChannelCategories.Should().Contain(c => c.Id == "1002").Subject;
        var channel3 = category2.Channels.Should().Contain(ch => ch.Channel.Id == "2205").Subject;
        channel3.Occupants.Should().HaveCount(1);
        channel3.Occupants.Should().Contain(u => u.Id == "3001");
    }

    [TestMethod]
    public void MoveUser_WhenTargetChannelDoesNotExist_OnlyRemovesFromCurrentChannel()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("9999");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        var sourceChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2001");
        sourceChannel.Occupants.Should().HaveCount(0);
        sourceChannel.Occupants.Should().NotContain(u => u.Id == "3001");

        var allUsers = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .SelectMany(ch => ch.Occupants);
        allUsers.Should().NotContain(u => u.Id == "3001");
    }

    [TestMethod]
    public void MoveUser_WhenMultipleUsersInChannel_OnlyMovesSpecifiedUser()
    {
        var occupants = GetDummyTownOccupants();
        var mockUser = CreateMockDiscordUser("3002");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        var sourceChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2002");
        sourceChannel.Occupants.Should().HaveCount(1);
        sourceChannel.Occupants.Should().Contain(u => u.Id == "3003");
        sourceChannel.Occupants.Should().NotContain(u => u.Id == "3002");


        var targetChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2001");
        targetChannel.Occupants.Should().HaveCount(2);
        targetChannel.Occupants.Should().Contain(u => u.Id == "3001");
        targetChannel.Occupants.Should().Contain(u => u.Id == "3002");
    }

    [TestMethod]
    public void MoveUser_PreservesUserCount()
    {
        var occupants = GetDummyTownOccupants();
        var initialUserCount = occupants.UserCount;
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("2002");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        result.UserCount.Should().Be(initialUserCount);
    }

    [TestMethod]
    public void MoveUser_WhenAddingNewUser_IncreasesUserCount()
    {
        var occupants = GetDummyTownOccupants();
        var initialUserCount = occupants.UserCount;
        var mockUser = CreateMockDiscordUser("3009");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(occupants, mockUser, mockChannel);

        result.UserCount.Should().Be(initialUserCount + 1);
    }

    [TestMethod]
    public void MoveUser_WhenRemovingUser_DecreasesUserCount()
    {
        var occupants = GetDummyTownOccupants();
        var initialUserCount = occupants.UserCount;
        var mockUser = CreateMockDiscordUser("3001");

        var result = _sut.MoveUser(occupants, mockUser, null);

        result.UserCount.Should().Be(initialUserCount - 1);
    }

    #endregion

    private static IDiscordGuildUser CreateMockDiscordUser(string id)
    {
        var mockUser = new Mock<IDiscordGuildUser>();
        mockUser.Setup(u => u.Id).Returns(ulong.Parse(id));
        mockUser.Setup(u => u.DisplayName).Returns(string.Empty);
        mockUser.Setup(u => u.DisplayAvatarUrl).Returns(string.Empty);
        mockUser.Setup(u => u.AsGameUser()).Returns(new GameUser(id, string.Empty, string.Empty));
        return mockUser.Object;
    }

    private static IDiscordVoiceChannel CreateMockDiscordVoiceChannel(string id)
    {
        var mockChannel = new Mock<IDiscordVoiceChannel>();
        mockChannel.Setup(c => c.Id).Returns(ulong.Parse(id));
        return mockChannel.Object;
    }
}