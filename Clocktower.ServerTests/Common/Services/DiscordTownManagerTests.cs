using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class DiscordTownManagerTests
{
    private Mock<IDiscordTownStore> _mockDiscordTownStore = null!;
    private Mock<IUserIdentityStore> _mockUserIdentityStore = null!;
    private Mock<IDiscordConstantsService> _mockDiscordConstantsService = null!;
    private IDiscordTownManager _sut = null!;
    private const string GuildId = "1";
    private DiscordTown _capturedDiscordTown = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordTownStore = StrictMockFactory.Create<IDiscordTownStore>();
        _mockUserIdentityStore = StrictMockFactory.Create<IUserIdentityStore>();
        _mockDiscordConstantsService = StrictMockFactory.Create<IDiscordConstantsService>();
        _mockDiscordConstantsService.SetupGet(s => s.NightCategoryName).Returns("Night Category");
        _sut = new DiscordTownManager(_mockDiscordTownStore.Object, _mockUserIdentityStore.Object, _mockDiscordConstantsService.Object);
    }

    private DiscordTown GetDummyDiscordTown()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var discordTown = new DiscordTown(channelCategories);

        _mockDiscordTownStore.Setup(o => o.Set(GuildId, It.IsAny<DiscordTown>(), true)).Returns(true)
            .Callback<string, DiscordTown, bool>((_, callbackTown, _) => _capturedDiscordTown = callbackTown);
        return discordTown;
    }

    private static readonly MiniCategory DayCategory = new("1001", "Day Category", [
        new ChannelOccupants(new MiniChannel("2001", "Day Channel 1"), [
            CommonMethods.GetRandomTownUser("3001")
        ]),
        new ChannelOccupants(new MiniChannel("2002", "Day Channel 2"), [
            CommonMethods.GetRandomTownUser("3002"),
            CommonMethods.GetRandomTownUser("3003")
        ])
    ]);

    private static readonly MiniCategory NightCategory = new("1002", "Night Category", [
        new ChannelOccupants(new MiniChannel("2203", "Night Channel 1"), []),
        new ChannelOccupants(new MiniChannel("2204", "Night Channel 2"), [
            CommonMethods.GetRandomTownUser("3004")
        ]),
        new ChannelOccupants(new MiniChannel("2205", "Night Channel 3"), []),
    ]);


    private static DiscordTown CreateEmptyDiscordTown() => new([]);


    #region FindUserChannel Tests

    [TestMethod]
    public void FindUserChannel_WhenUserExists_ReturnsCorrectChannel()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        const string userId = "3001";

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

        result.Should().NotBeNull();
        result.Channel.Id.Should().Be("2001");
        result.Channel.Name.Should().Be("Day Channel 1");
        result.Occupants.Should().Contain(u => u.Id == userId);
    }

    [TestMethod]
    public void FindUserChannel_WhenUserExistsInSecondChannel_ReturnsCorrectChannel()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        const string userId = "3003";

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

        result.Should().NotBeNull();
        result.Channel.Id.Should().Be("2002");
        result.Channel.Name.Should().Be("Day Channel 2");
        result.Occupants.Should().Contain(u => u.Id == userId);
    }

    [TestMethod]
    public void FindUserChannel_WhenUserDoesNotExist_ReturnsNull()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        const string userId = "nonexistent";

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenDiscordTownIsEmpty_ReturnsNull()
    {
        var dummyDiscordTown = CreateEmptyDiscordTown();
        const string userId = "3001";

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenUserIdIsEmpty_ReturnsNull()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        const string userId = "";

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenUserIdIsNull_ReturnsNull()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        string userId = null!;

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void FindUserChannel_WhenChannelHasMultipleUsers_ReturnsChannelWithAllUsers()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        const string userId = "3002";

        var result = _sut.FindUserChannel(dummyDiscordTown, userId);

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
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3005");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

        var targetChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2001");

        targetChannel.Occupants.Should().HaveCount(2);
        targetChannel.Occupants.Should().Contain(u => u.Id == "3005");
    }

    [TestMethod]
    public void MoveUser_WhenUserInSameChannel_ReturnsUnchanged()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

        result.Should().BeSameAs(dummyDiscordTown);
    }

    [TestMethod]
    public void MoveUser_WhenMovingBetweenChannels_UpdatesBothChannels()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3003");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

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
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3003");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, null);

        var sourceChannel = result.ChannelCategories
            .SelectMany(c => c.Channels)
            .First(ch => ch.Channel.Id == "2002");
        sourceChannel.Occupants.Should().HaveCount(1);
        sourceChannel.Occupants.Should().NotContain(u => u.Id == "3003");
    }

    [TestMethod]
    public void MoveUser_WhenMovingToNewChannelInDifferentCategory_UpdatesCorrectCategories()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("2205");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

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
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("9999");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

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
        var dummyDiscordTown = GetDummyDiscordTown();
        var mockUser = CreateMockDiscordUser("3002");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

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
        var dummyDiscordTown = GetDummyDiscordTown();
        var initialUserCount = dummyDiscordTown.UserCount;
        var mockUser = CreateMockDiscordUser("3001");
        var mockChannel = CreateMockDiscordVoiceChannel("2002");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

        result.UserCount.Should().Be(initialUserCount);
    }

    [TestMethod]
    public void MoveUser_WhenAddingNewUser_IncreasesUserCount()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        var initialUserCount = dummyDiscordTown.UserCount;
        var mockUser = CreateMockDiscordUser("3009");
        var mockChannel = CreateMockDiscordVoiceChannel("2001");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, mockChannel);

        result.UserCount.Should().Be(initialUserCount + 1);
    }

    [TestMethod]
    public void MoveUser_WhenRemovingUser_DecreasesUserCount()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        var initialUserCount = dummyDiscordTown.UserCount;
        var mockUser = CreateMockDiscordUser("3001");

        var result = _sut.MoveUser(dummyDiscordTown, mockUser, null);

        result.UserCount.Should().Be(initialUserCount - 1);
    }

    #endregion

    #region GetDiscordTown

    [TestMethod]
    public void GetDiscordTown_ReturnsTown()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetDiscordTown(GuildId);

        result.Should().Be(town);
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    #endregion

    #region GetTownUser

    private void Setup_GetTownUser(string userId, DiscordTown? discordTown)
    {
        _mockDiscordTownStore.Setup(o => o.GetTownByUser(userId)).Returns(discordTown);
    }

    [TestMethod]
    public void GetTownUser_ReturnsNull_WhenNoTownFound()
    {
        const string userId = "3003";
        Setup_GetTownUser(userId, null);

        var result = _sut.GetTownUser(userId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetTownUser_ReturnsUser_WhenTownContainsUser()
    {
        const string userId = "3003";
        var town = GetDummyDiscordTown();
        Setup_GetTownUser(userId, town);

        var result = _sut.GetTownUser(userId);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
    }

    [TestMethod]
    public void GetTownUser_ReturnsNull_WhenTownNotContainUser()
    {
        const string userId = "9999";
        var town = GetDummyDiscordTown();
        Setup_GetTownUser(userId, town);

        var result = _sut.GetTownUser(userId);

        result.Should().BeNull();
    }

    #endregion

    #region UpdateUserStatus

    private void Setup_UpdateUserStatus(DiscordTown? discordTown)
    {
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(discordTown);
    }

    [TestMethod]
    public void UpdateUserStatus_ReturnsFalse_WhenNoTownFound()
    {
        const string userId = "3003";
        const bool isPresent = true;
        VoiceState voiceState = new VoiceState(true, false, true, false);
        Setup_UpdateUserStatus(null);

        var result = _sut.UpdateUserStatus(GuildId, userId, isPresent, voiceState);

        result.Should().BeFalse();
    }


    [TestMethod]
    public void UpdateUserStatus_CallsSetState_WhenTownFound()
    {
        const string userId = "3003";
        const bool isPresent = true;
        VoiceState voiceState = new VoiceState(true, false, true, false);
        var town = GetDummyDiscordTown();
        Setup_UpdateUserStatus(town);

        var result = _sut.UpdateUserStatus(GuildId, userId, isPresent, voiceState);

        result.Should().BeTrue();
        var updatedUser = _capturedDiscordTown.TownUsers.FirstOrDefault(townUser => townUser.Id == userId);

        updatedUser.Should().NotBeNull();
        updatedUser.IsPresent.Should().Be(isPresent);
        updatedUser.VoiceState.Should().Be(voiceState);
    }

    #endregion

    #region GetNightChannels

    [TestMethod]
    public void GetNightChannels_ReturnsEmpty_WhenTownIsNull()
    {
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns((DiscordTown?)null);

        var result = _sut.GetNightChannels(GuildId, "Night Category").ToArray();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetNightChannels_ReturnsEmpty_WhenCategoryNotFound()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetNightChannels(GuildId, "Does Not Exist").ToArray();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetNightChannels_ReturnsChannelsFromMatchingCategory()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetNightChannels(GuildId, "Night Category").ToList();

        result.Should().HaveCount(3);
        result.Select(c => c.Id).Should().BeEquivalentTo("2203", "2204", "2205");
        result.Select(c => c.Name).Should().BeEquivalentTo("Night Channel 1", "Night Channel 2", "Night Channel 3");
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    #endregion

    #region GetVoiceChannelIdByName

    [TestMethod]
    public void GetVoiceChannelIdByName_ReturnsNull_WhenTownIsNull()
    {
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns((DiscordTown?)null);

        var result = _sut.GetVoiceChannelIdByName(GuildId, "Night Channel 1");

        result.Should().BeNull();
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetVoiceChannelIdByName_ReturnsNull_WhenChannelNameNotFound()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetVoiceChannelIdByName(GuildId, "Does Not Exist");

        result.Should().BeNull();
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetVoiceChannelIdByName_ReturnsChannelId_WhenChannelExistsInDayCategory()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetVoiceChannelIdByName(GuildId, "Day Channel 2");

        result.Should().Be("2002");
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetVoiceChannelIdByName_ReturnsChannelId_WhenChannelExistsInNightCategory()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetVoiceChannelIdByName(GuildId, "Night Channel 2");

        result.Should().Be("2204");
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetVoiceChannelIdByName_ReturnsNull_WhenVoiceChannelNameIsEmpty()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);

        var result = _sut.GetVoiceChannelIdByName(GuildId, "");

        result.Should().BeNull();
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    [TestMethod]
    public void GetVoiceChannelIdByName_ReturnsNull_WhenVoiceChannelNameIsNull()
    {
        var town = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(town);
        string voiceChannelName = null!;

        var result = _sut.GetVoiceChannelIdByName(GuildId, voiceChannelName);

        result.Should().BeNull();
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
    }

    #endregion

    #region RedactTownDto

    [TestMethod]
    public void RedactTownDto_WhenUserIsInNightChannel_ReturnsOnlyThatNightChannel_AndKeepsOtherCategories()
    {
        const string gameId = "test-game-123";
        const string userId = "u-night";

        var dayCategory = new MiniCategoryDto("day", "Day Category", [
            new ChannelOccupantsDto(
                new MiniChannel("day-1", "Day Channel 1"),
                [new UserDto("u-day", "Day User", "avatar")]
            )
        ]);

        var nightCategory = new MiniCategoryDto("night", "Night Category", [
            new ChannelOccupantsDto(
                new MiniChannel("night-1", "Night Channel 1"),
                [new UserDto("someone-else", "Other", "avatar")]
            ),
            new ChannelOccupantsDto(
                new MiniChannel("night-2", "Night Channel 2"),
                [new UserDto(userId, "Viewer", "avatar")]
            ),
            new ChannelOccupantsDto(
                new MiniChannel("night-3", "Night Channel 3"),
                []
            )
        ]);

        var dto = new DiscordTownDto(gameId, [dayCategory, nightCategory]);

        var result = _sut.RedactTownDto(dto, userId);

        result.GameId.Should().Be(gameId);

        result.ChannelCategories.Should().ContainSingle(c => c.Name == "Day Category");
        result.ChannelCategories.Should().ContainSingle(c => c.Name == "Night Category");

        var redactedNight = result.ChannelCategories.Single(c => c.Name == "Night Category");
        redactedNight.Channels.Should().HaveCount(1);
        redactedNight.Channels.Single().Channel.Id.Should().Be("night-2");
        redactedNight.Channels.Single().Occupants.Should().Contain(o => o.Id == userId);
    }

    [TestMethod]
    public void RedactTownDto_WhenUserNotInAnyNightChannel_RemovesNightCategory()
    {
        const string gameId = "test-game-123";
        const string userId = "u-not-in-night";

        var dayCategory = new MiniCategoryDto("day", "Day Category", [
            new ChannelOccupantsDto(
                new MiniChannel("day-1", "Day Channel 1"),
                [new UserDto("u-day", "Day User", "avatar")]
            )
        ]);

        var nightCategory = new MiniCategoryDto("night", "Night Category", [
            new ChannelOccupantsDto(
                new MiniChannel("night-1", "Night Channel 1"),
                [new UserDto("someone-else", "Other", "avatar")]
            )
        ]);

        var dto = new DiscordTownDto(gameId, [dayCategory, nightCategory]);

        var result = _sut.RedactTownDto(dto, userId);

        result.ChannelCategories.Should().ContainSingle(c => c.Name == "Day Category");
        result.ChannelCategories.Should().NotContain(c => c.Name == "Night Category");
    }

    [TestMethod]
    public void RedactTownDto_MatchesNightCategoryName_CaseInsensitive()
    {
        const string gameId = "test-game-123";
        const string userId = "u-night";

        _mockDiscordConstantsService
            .SetupGet(s => s.NightCategoryName)
            .Returns("nIgHt CaTeGoRy");

        _sut = new DiscordTownManager(_mockDiscordTownStore.Object, _mockUserIdentityStore.Object, _mockDiscordConstantsService.Object);

        var dayCategory = new MiniCategoryDto("day", "Day Category", [
            new ChannelOccupantsDto(
                new MiniChannel("day-1", "Day Channel 1"),
                [new UserDto("u-day", "Day User", "avatar")]
            )
        ]);

        var nightCategory = new MiniCategoryDto("night", "NIGHT CATEGORY", [
            new ChannelOccupantsDto(
                new MiniChannel("night-2", "Night Channel 2"),
                [new UserDto(userId, "Viewer", "avatar")]
            ),
            new ChannelOccupantsDto(
                new MiniChannel("night-1", "Night Channel 1"),
                [new UserDto("someone-else", "Other", "avatar")]
            )
        ]);

        var dto = new DiscordTownDto(gameId, [dayCategory, nightCategory]);

        var result = _sut.RedactTownDto(dto, userId);

        result.ChannelCategories.Should().ContainSingle(c => c.Name == "Day Category");
        result.ChannelCategories.Should().ContainSingle(c => c.Name == "NIGHT CATEGORY");

        var redactedNight = result.ChannelCategories.Single(c => c.Name == "NIGHT CATEGORY");
        redactedNight.Channels.Should().HaveCount(1);
        redactedNight.Channels.Single().Channel.Id.Should().Be("night-2");
    }

    #endregion

    #region SetDiscordTown

    [TestMethod]
    public void SetDiscordTown_SetsTownCorrectly()
    {
        const string gameId = "test-game-123";
        var discordTown = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Set(gameId, discordTown)).Returns(true);

        var result = _sut.SetDiscordTown(gameId, discordTown);

        result.Should().BeTrue();
        _mockDiscordTownStore.Verify(o => o.Set(gameId, discordTown), Times.Once);
    }

    #endregion

    #region UpdateUserIdentity

    [TestMethod]
    public void UpdateUserIdentity_UpdatesIdentity()
    {
        var townUser = CommonMethods.GetRandomTownUser();
        _mockUserIdentityStore.Setup(o => o.UpdateIdentity(townUser));

        _sut.UpdateUserIdentity(townUser);

        _mockUserIdentityStore.Verify(o => o.UpdateIdentity(townUser), Times.Once);
    }

    #endregion

    #region GetDiscordTownDto

    [TestMethod]
    public void GetDiscordTownDto_Id_ReturnsNull_WhenNoTownFound()
    {
        const string guildId = "123";
        const string gameId = "456";
        _mockDiscordTownStore.Setup(o => o.Get(guildId)).Returns((DiscordTown?)null);

        var result = _sut.GetDiscordTownDto(guildId, gameId);

        result.Should().BeNull();
        _mockDiscordTownStore.Verify(o => o.Get(guildId), Times.Once);
    }

    [TestMethod]
    public void GetDiscordTownDto_ReturnsNull_WhenNoTownFound()
    {
        const string gameId = "456";

        var result = _sut.GetDiscordTownDto((DiscordTown?)null, gameId);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetDiscordTownDto_Id_UpdatesUsers()
    {
        const string guildId = "123";
        const string gameId = "456";
        var discordTown = GetDummyDiscordTown();
        var gameUsers = new List<GameUser>
        {
            CommonMethods.GetRandomGameUser("3001"),
            CommonMethods.GetRandomGameUser("3002"),
            CommonMethods.GetRandomGameUser("3003"),
            CommonMethods.GetRandomGameUser("3004")
        };
        foreach (var user in gameUsers) _mockUserIdentityStore.Setup(o => o.GetIdentity(user.Id)).Returns(CommonMethods.GetRandomTownUser(user.Id));
        foreach (var user in discordTown.TownUsers) _mockUserIdentityStore.Setup(o => o.UpdateIdentity(user));
        _mockDiscordTownStore.Setup(o => o.Get(guildId)).Returns(discordTown);

        var result = _sut.GetDiscordTownDto(guildId, gameId, gameUsers);

        result.Should().NotBeNull();
        foreach (var user in discordTown.TownUsers) _mockUserIdentityStore.Verify(o => o.UpdateIdentity(user), Times.Once);
        foreach (var user in gameUsers) _mockUserIdentityStore.Verify(o => o.GetIdentity(user.Id), Times.Once);
    }

    #endregion


    private static IDiscordGuildUser CreateMockDiscordUser(string id)
    {
        var mockUser = StrictMockFactory.Create<IDiscordGuildUser>();
        mockUser.Setup(u => u.Id).Returns(id);
        mockUser.Setup(u => u.GuildId).Returns(GuildId);
        mockUser.Setup(u => u.DisplayName).Returns(string.Empty);
        mockUser.Setup(u => u.DisplayAvatarUrl).Returns(string.Empty);
        mockUser.Setup(u => u.AsTownUser()).Returns(CommonMethods.GetRandomTownUser(id));
        return mockUser.Object;
    }

    private static IDiscordVoiceChannel CreateMockDiscordVoiceChannel(string id)
    {
        var mockChannel = StrictMockFactory.Create<IDiscordVoiceChannel>();
        mockChannel.Setup(c => c.Id).Returns(id);
        return mockChannel.Object;
    }
}