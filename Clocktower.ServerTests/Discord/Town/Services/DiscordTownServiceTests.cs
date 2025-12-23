using Clocktower.Server.Common;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Clocktower.ServerTests.Discord.Town.Services;

[TestClass]
public class DiscordTownServiceTests
{
    private const string GuildId = "1";
    private const string UserId = "2";
    private const string ChannelId = "3";
    private const string RoleId = "4";
    private const string DayCategoryId = "5";
    private const string NightCategoryId = "6";
    private const string GameId = "this-game";
    private const string DisplayName = "displayName";

    private const string Jwt = "jwt-token";
    private const string Key = "this-key";
    private const string VoiceChannelName = "voice channel name";
    private const string DayCategoryName = "dayCategoryName";
    private const string NightCategoryName = "nightCategoryName";
    private const string ExceptionMessage = "message";
    private const string GuildNotFoundMessage = "Guild not found";
    private const string StoryTellerRoleName = "StoryTellerRoleName";
    private static readonly string[] DayChannelNames = ["day-1", "day-2", "day-3"];
    private static readonly string[] NightChannelNames = ["night-1", "night-2", "night-3"];

    private const string FeUrl = "fe-url";

    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IGamePerspectiveStore> _mockGamePerspectiveStore = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private Mock<IMemoryCache> _mockCache = null!;
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private Mock<IDiscordConstantsService> _mockDiscordConstants = null!;
    private Mock<IIdGenerator> _mockIdGenerator = null!;
    private IDiscordTownService _sut = null!;


    private Mock<IDiscordGuild> _guild = null!;
    private Mock<IDiscordRole> _role = null!;
    private Mock<IDiscordGuildUser> _user = null!;
    private Mock<IDiscordCategoryChannel> _dayCategory = null!;
    private Mock<IDiscordCategoryChannel> _nightCategory = null!;
    private Mock<IDiscordRestCategoryChannel> _dayRestCategory = null!;
    private Mock<IDiscordRestCategoryChannel> _nightRestCategory = null!;
    private Mock<IDiscordVoiceState> _voiceState = null!;
    private Mock<IDiscordVoiceChannel> _voiceChannel = null!;
    private Mock<IDiscordDmChannel> _dmChannel = null!;
    private Mock<ICacheEntry> _cacheEntry = null!;

    private GameUser _gameUser = null!;
    private DiscordTown _discordTown = null!;

    [TestInitialize]
    public void Setup()
    {
        _guild = StrictMockFactory.Create<IDiscordGuild>();
        _role = StrictMockFactory.Create<IDiscordRole>();
        _user = StrictMockFactory.Create<IDiscordGuildUser>();
        _dayCategory = StrictMockFactory.Create<IDiscordCategoryChannel>();
        _nightCategory = StrictMockFactory.Create<IDiscordCategoryChannel>();
        _dayRestCategory = StrictMockFactory.Create<IDiscordRestCategoryChannel>();
        _nightRestCategory = StrictMockFactory.Create<IDiscordRestCategoryChannel>();
        _voiceState = StrictMockFactory.Create<IDiscordVoiceState>();
        _voiceChannel = StrictMockFactory.Create<IDiscordVoiceChannel>();
        _dmChannel = StrictMockFactory.Create<IDiscordDmChannel>();
        _cacheEntry = StrictMockFactory.Create<ICacheEntry>();

        _mockBot = new Mock<IDiscordBot>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockGamePerspectiveStore = new Mock<IGamePerspectiveStore>();
        _mockDiscordTownManager = new Mock<IDiscordTownManager>();
        _mockJwtWriter = new Mock<IJwtWriter>();
        _mockCache = new Mock<IMemoryCache>();
        _mockSecrets = new Mock<IOptions<Secrets>>();
        _mockDiscordConstants = new Mock<IDiscordConstantsService>();
        _mockIdGenerator = new Mock<IIdGenerator>();

        CommonMethods.SetUpMockSecrets(_mockSecrets, feUri: FeUrl);

        _sut = new DiscordTownService(
            _mockBot.Object,
            _mockNotificationService.Object,
            _mockGamePerspectiveStore.Object,
            _mockDiscordTownManager.Object,
            _mockJwtWriter.Object,
            _mockCache.Object,
            _mockSecrets.Object,
            _mockDiscordConstants.Object,
            _mockIdGenerator.Object
        );
    }


    #region MoveUser

    public void Setup_MoveUser(bool hasGuild = false, bool hasUser = false, bool hasVoiceState = false, bool hasVoiceChannel = false)
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);
        _guild.Setup(o => o.GetUser(UserId)).Returns(hasUser ? _user.Object : null);
        _user.Setup(o => o.VoiceState).Returns(hasVoiceState ? _voiceState.Object : null);
        _guild.Setup(o => o.GetVoiceChannel(ChannelId)).Returns(hasVoiceChannel ? _voiceChannel.Object : null);
        _voiceChannel.Setup(o => o.Name).Returns(VoiceChannelName);

        _user.Setup(o => o.DisplayName).Returns(DisplayName);
        _user.Setup(o => o.MoveAsync(_voiceChannel.Object)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenGuildNotFound()
    {
        Setup_MoveUser(hasGuild: false);

        var (success, message) = await _sut.MoveUser(GuildId, UserId, ChannelId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be(GuildNotFoundMessage);
    }


    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenUserNotFound()
    {
        Setup_MoveUser(hasGuild: true, hasUser: false);

        var (success, message) = await _sut.MoveUser(GuildId, UserId, ChannelId);

        _guild.Verify(o => o.GetUser(UserId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be("User not found in guild");
    }

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenUserNotInVoice()
    {
        Setup_MoveUser(hasGuild: true, hasUser: true, hasVoiceState: false);

        var (success, message) = await _sut.MoveUser(GuildId, UserId, ChannelId);

        success.Should().BeFalse();
        message.Should().Be("User is not connected to voice");
    }


    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenChannelNotFound()
    {
        Setup_MoveUser(hasGuild: true, hasUser: true, hasVoiceState: true, hasVoiceChannel: false);

        var (success, message) = await _sut.MoveUser(GuildId, UserId, ChannelId);

        _guild.Verify(o => o.GetVoiceChannel(ChannelId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be("Channel not found in guild");
    }


    [TestMethod]
    public async Task MoveUser_ReturnsTrueMovesUser_WhenDataGood()
    {
        Setup_MoveUser(hasGuild: true, hasUser: true, hasVoiceState: true, hasVoiceChannel: true);

        var (success, message) = await _sut.MoveUser(GuildId, UserId, ChannelId);

        _user.Verify(o => o.MoveAsync(_voiceChannel.Object), Times.Once);
        success.Should().BeTrue();
        message.Should().Be($"User {DisplayName} moved to {VoiceChannelName}");
    }

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenExceptionThrown()
    {
        const string exceptionMessage = "something went wrong";
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(exceptionMessage));

        var (success, message) = await _sut.MoveUser(GuildId, UserId, ChannelId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be(exceptionMessage);
    }

    #endregion

    #region ToggleStoryTeller

    private void Setup_ToggleStoryTeller(bool hasGamePerspective, bool hasGuild = false, bool hasRole = false, bool hasUser = false, bool hasPerspectiveUser = false, bool userHasRole = false)
    {
        _gameUser = CommonMethods.GetRandomGameUser(UserId);

        _role.Setup(o => o.Id).Returns(RoleId);
        _user.Setup(o => o.Id).Returns(UserId);
        _user.Setup(o => o.Roles).Returns([]);
        _user.Setup(o => o.AsGameUser()).Returns(_gameUser);
        _user.Setup(o => o.DisplayName).Returns(DisplayName);
        _user.Setup(o => o.DoesUserHaveRole(RoleId)).Returns(userHasRole);

        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);
        _guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(hasRole ? _role.Object : null);
        _guild.Setup(o => o.GetUser(UserId)).Returns(hasUser ? _user.Object : null);


        var gameUsers = hasPerspectiveUser ? [CommonMethods.GetRandomGameUser(UserId)] : Array.Empty<GameUser>();
        GamePerspective gamePerspective = CommonMethods.GetGamePerspective(GameId, guildId: GuildId) with { Users = gameUsers };

        _mockGamePerspectiveStore.Setup(o => o.GetFirstPerspective(GameId))
            .Returns(hasGamePerspective ? gamePerspective : null);
        _mockGamePerspectiveStore.Setup(o => o.Get(GameId, UserId))
            .Returns(hasPerspectiveUser ? gamePerspective : null);

        _user.Setup(o => o.AddRoleAsync(_role.Object)).Returns(Task.CompletedTask);
        _user.Setup(o => o.RemoveRoleAsync(_role.Object)).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenNoGameFound()
    {
        Setup_ToggleStoryTeller(false);

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        _mockGamePerspectiveStore.Verify(o => o.GetFirstPerspective(GameId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be($"Couldn't find game with id: {GameId}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenGuildNotFound()
    {
        Setup_ToggleStoryTeller(true, hasGuild: false);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenRoleNotFound()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: false);

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        _guild.Verify(o => o.GetRole(StoryTellerRoleName), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be($"{StoryTellerRoleName} role does not exist");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenUserNotFound()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: true, hasUser: false);

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        _guild.Verify(o => o.GetUser(UserId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("User not found");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_CallsDoesUserHaveRole_WhenDataGood()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: true, hasUser: true);

        _ = await _sut.ToggleStoryTeller(GameId, UserId);

        _user.Verify(o => o.DoesUserHaveRole(RoleId), Times.Once);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_AddsUserToGame_WhenUserNotInGamePerspective()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: true, hasUser: true, hasPerspectiveUser: false);

        _ = await _sut.ToggleStoryTeller(GameId, UserId);

        _mockGamePerspectiveStore.Verify(o => o.GetFirstPerspective(GameId), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.Get(GameId, UserId), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.AddUserToGame(GameId, _gameUser), Times.Once);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_DoesNotUserToGame_WhenUserIsInGamePerspective()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: true, hasUser: true, hasPerspectiveUser: true);

        _ = await _sut.ToggleStoryTeller(GameId, UserId);

        _mockGamePerspectiveStore.Verify(o => o.GetFirstPerspective(GameId), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.Get(GameId, UserId), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.AddUserToGame(It.IsAny<string>(), It.IsAny<GameUser>()), Times.Never);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_AddsStoryTellerRole_WhenUserIsNotStoryTeller()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: true, hasUser: true, hasPerspectiveUser: true, userHasRole: false);

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        _user.Verify(o => o.AddRoleAsync(_role.Object), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.UpdateUser(GameId, UserId, UserType.StoryTeller), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be($"User {DisplayName} is now a {StoryTellerRoleName}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_RemovesStoryTellerRole_WhenUserIsStoryTeller()
    {
        Setup_ToggleStoryTeller(true, hasGuild: true, hasRole: true, hasUser: true, hasPerspectiveUser: true, userHasRole: true);

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        _user.Verify(o => o.RemoveRoleAsync(_role.Object), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.UpdateUser(GameId, UserId, UserType.Spectator), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be($"User {DisplayName} is no longer a {StoryTellerRoleName}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenExceptionThrown()
    {
        const string exceptionMessage = "exceptionMessage";
        _mockGamePerspectiveStore.Setup(o => o.GetFirstPerspective(GameId)).Throws(new Exception(exceptionMessage));

        var result = await _sut.ToggleStoryTeller(GameId, UserId);

        result.success.Should().BeFalse();
        result.message.Should().Be(exceptionMessage);
    }

    #endregion


    #region GetJoinData

    [TestMethod]
    public void GetJoinData_ReturnsNull_WhenKeyNotFound()
    {
        object invalidValue = null!;
        _mockCache.Setup(o => o.TryGetValue($"join_data_{Key}", out invalidValue!)).Returns(false);

        var result = _sut.GetJoinData(Key);

        _mockCache.Verify(o => o.TryGetValue($"join_data_{Key}", out It.Ref<object?>.IsAny), Times.Once);
        result.Should().BeNull();
    }


    [TestMethod]
    public void GetJoinData_ReturnsNull_WhenCacheValueInvalid()
    {
        object invalidValue = "not-a-_user-join-data-object";
        _mockCache.Setup(o => o.TryGetValue($"join_data_{Key}", out invalidValue!)).Returns(true);

        var result = _sut.GetJoinData(Key);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetJoinData_CallsCacheRemove_UpdatesGame_ReturnsData_WhenCacheValueFound()
    {
        var joinData = new JoinData(CommonMethods.GetRandomString(), CommonMethods.GetRandomGameUser(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString());
        object joinDataObj = joinData;
        _mockCache.Setup(o => o.TryGetValue($"join_data_{Key}", out joinDataObj!)).Returns(true);

        var result = _sut.GetJoinData(Key);

        _mockGamePerspectiveStore.Verify(o => o.UpdateUser(joinData.GameId, joinData.User.Id, null, true), Times.Once());
        _mockCache.Verify(o => o.Remove($"join_data_{Key}"), Times.Once);
        result.Should().Be(joinData);
    }

    #endregion


    #region PingUser

    [TestMethod]
    public async Task PingUser_PingsUser()
    {
        await _sut.PingUser(UserId);

        _mockNotificationService.Verify(o => o.PingUser(UserId, "Ping!"), Times.Once);
    }

    #endregion

    #region DeleteTown

    private void Setup_DeleteTown(bool hasGuild = false)
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);

        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(_dayCategory.Object);
        _guild.Setup(o => o.GetCategoryChannelByName(NightCategoryName)).Returns(_nightCategory.Object);

        _guild.Setup(o => o.DeleteRoleAsync(StoryTellerRoleName)).Returns(Task.CompletedTask);
        _dayCategory.Setup(o => o.DeleteAsync()).Returns(Task.CompletedTask);
        _nightCategory.Setup(o => o.DeleteAsync()).Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsFalse_WhenGuildNotFound()
    {
        Setup_DeleteTown(hasGuild: false);

        var result = await _sut.DeleteTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsTrue_WhenGuildFound()
    {
        Setup_DeleteTown(hasGuild: true);

        var result = await _sut.DeleteTown(GuildId);

        _guild.Verify(o => o.GetCategoryChannelByName(DayCategoryName), Times.Once);
        _dayCategory.Verify(o => o.DeleteAsync(), Times.Once);
        _guild.Verify(o => o.GetCategoryChannelByName(NightCategoryName), Times.Once);
        _nightCategory.Verify(o => o.DeleteAsync(), Times.Once);
        _guild.Verify(o => o.DeleteRoleAsync(StoryTellerRoleName), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be("Town deleted");
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsFalse_WhenExceptionThrown()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(ExceptionMessage));

        var result = await _sut.DeleteTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(ExceptionMessage);
    }

    #endregion

    #region CreateTown

    private void Setup_CreateTown(bool hasGuild = false, bool roleCreated = false, bool dayChannelsCreated = false, bool nightChannelsCreated = false)
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);

        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(NightChannelNames);

        _dayRestCategory.Setup(o => o.Id).Returns(DayCategoryId);
        _nightRestCategory.Setup(o => o.Id).Returns(NightCategoryId);

        _guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, It.IsAny<Color>())).ReturnsAsync(roleCreated ? _role.Object : null!);
        _guild.Setup(o => o.CreateCategoryAsync(DayCategoryName, everyoneCanSee: true)).ReturnsAsync(_dayRestCategory.Object);
        _guild.Setup(o => o.CreateCategoryAsync(NightCategoryName, everyoneCanSee: false, _role.Object)).ReturnsAsync(_nightRestCategory.Object);
        _guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(DayChannelNames, DayCategoryId)).ReturnsAsync(dayChannelsCreated);
        _guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(NightChannelNames, NightCategoryId)).ReturnsAsync(nightChannelsCreated);
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenGuildNotFound()
    {
        Setup_CreateTown(hasGuild: false);

        var result = await _sut.CreateTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenExceptionThrown()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(ExceptionMessage));

        var result = await _sut.CreateTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(ExceptionMessage);
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenRoleNotCreated()
    {
        Setup_CreateTown(hasGuild: true, roleCreated: false);

        var result = await _sut.CreateTown(GuildId);

        _guild.Verify(o => o.CreateRoleAsync(StoryTellerRoleName, It.IsAny<Color>()), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to create role");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenDayChannelsNotCreated()
    {
        Setup_CreateTown(hasGuild: true, roleCreated: true, dayChannelsCreated: false);

        var result = await _sut.CreateTown(GuildId);

        _guild.Verify(o => o.CreateCategoryAsync(DayCategoryName, everyoneCanSee: true), Times.Once);
        _guild.Verify(o => o.CreateVoiceChannelsForCategoryAsync(DayChannelNames, DayCategoryId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to generate day channels");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenNightChannelsNotCreated()
    {
        Setup_CreateTown(hasGuild: true, roleCreated: true, dayChannelsCreated: true, nightChannelsCreated: false);

        var result = await _sut.CreateTown(GuildId);

        _guild.Verify(o => o.CreateCategoryAsync(NightCategoryName, everyoneCanSee: false, _role.Object), Times.Once);
        _guild.Verify(o => o.CreateVoiceChannelsForCategoryAsync(NightChannelNames, NightCategoryId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to generate night channels");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsTrue_WhenChannelsCreated()
    {
        Setup_CreateTown(hasGuild: true, roleCreated: true, dayChannelsCreated: true, nightChannelsCreated: true);

        var result = await _sut.CreateTown(GuildId);

        result.success.Should().BeTrue();
        result.message.Should().Be("Town created");
    }

    #endregion

    #region GetTownStatus

    private void Setup_GetTownStatus(bool hasGuild = false, bool roleFound = false, bool hasDayCategory = false, bool hasDayChannels = false, bool hasNightCategory = false, bool hasNightChannels = false)
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);

        _guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(roleFound ? _role.Object : null);
        _guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(hasDayCategory ? _dayCategory.Object : null);
        _guild.Setup(o => o.GetCategoryChannelByName(NightCategoryName)).Returns(hasNightCategory ? _nightCategory.Object : null);

        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(NightChannelNames);

        _dayCategory.Setup(o => o.VerifyCategoryChannels(DayChannelNames)).Returns(hasDayChannels);
        _nightCategory.Setup(o => o.VerifyCategoryChannels(NightChannelNames)).Returns(hasNightChannels);
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenExceptionThrown()
    {
        const string exMessage = "message";
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(exMessage));

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeFalse();
        exists.Should().BeFalse();
        message.Should().Be(exMessage);
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenGuildNotFound()
    {
        Setup_GetTownStatus(hasGuild: false);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeFalse();
        exists.Should().BeFalse();
        message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenStoryTellerRoleNotFound()
    {
        Setup_GetTownStatus(hasGuild: true, roleFound: false);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be($"{StoryTellerRoleName} role does not exist");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenDayCategoryMissing()
    {
        Setup_GetTownStatus(hasGuild: true, roleFound: true, hasDayCategory: false);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing day category");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenDayChannelsNotMatch()
    {
        Setup_GetTownStatus(hasGuild: true, roleFound: true, hasDayCategory: true, hasDayChannels: false);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing day channels");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenNightCategoryMissing()
    {
        Setup_GetTownStatus(hasGuild: true, roleFound: true, hasDayCategory: true, hasDayChannels: true, hasNightCategory: false);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing night category");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenNightChannelsNotMatch()
    {
        Setup_GetTownStatus(hasGuild: true, roleFound: true, hasDayCategory: true, hasDayChannels: true, hasNightCategory: true, hasNightChannels: false);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Not enough cottages");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsTrue_WhenTownStructureCorrect()
    {
        Setup_GetTownStatus(hasGuild: true, roleFound: true, hasDayCategory: true, hasDayChannels: true, hasNightCategory: true, hasNightChannels: true);

        var (success, exists, message) = _sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeTrue();
        message.Should().Be("Town structure is correct");
    }

    #endregion


    #region GetDiscordTown

    private void Setup_GetDiscordTown(bool hasCachedTown = false, bool hasGuild = false, bool hasDayCategory = false, bool hasNightCategory = false, bool gameFound = false)
    {
        _discordTown = GetDummyDiscordTown(hasDayCategory, hasNightCategory);

        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);

        _mockDiscordTownManager.Setup(o => o.GetDiscordTown(GuildId)).Returns(hasCachedTown ? _discordTown : null);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);

        _guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(hasDayCategory ? DayCategory : null);
        _guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(hasNightCategory ? NightCategory : null);

        _mockGamePerspectiveStore.Setup(o => o.GetGuildGameIds(GuildId)).Returns(gameFound ? [GameId] : []);
    }

    [TestMethod]
    public async Task GetDiscordTown_ReturnsFalse_WhenExceptionThrown()
    {
        const string exMessage = "message";
        _mockDiscordTownManager.Setup(o => o.GetDiscordTown(GuildId)).Throws(new Exception(exMessage));

        var (success, discordTown, message) = await _sut.GetDiscordTown(GuildId);

        success.Should().BeFalse();
        discordTown.Should().BeNull();
        message.Should().Be(exMessage);
    }

    [TestMethod]
    public async Task GetDiscordTown_ReturnsTrue_WhenStoreHasValue()
    {
        Setup_GetDiscordTown(hasCachedTown: true);

        var (success, discordTown, message) = await _sut.GetDiscordTown(GuildId);

        success.Should().BeTrue();
        discordTown.Should().Be(_discordTown);
        message.Should().Be("Got from store");
    }

    [TestMethod]
    public async Task GetDiscordTown_ReturnsFalse_WhenGuildNotFound()
    {
        Setup_GetDiscordTown(hasCachedTown: false, hasGuild: false);

        var (success, discordTown, message) = await _sut.GetDiscordTown(GuildId);

        success.Should().BeFalse();
        discordTown.Should().Be(null);
        message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(false, false)]
    public async Task GetDiscordTown_SetsStore_WithExpectedCategories(bool hasDayCategory, bool hasNightCategory)
    {
        Setup_GetDiscordTown(hasCachedTown: false, hasGuild: true, hasDayCategory: hasDayCategory, hasNightCategory: hasNightCategory);

        var (success, discordTown, message) = await _sut.GetDiscordTown(GuildId);

        success.Should().BeTrue();
        discordTown.Should().BeEquivalentTo(_discordTown);
        message.Should().Be($"Discord town {discordTown.UserCount}");
    }

    [TestMethod]
    public async Task GetDiscordTown_DoesNotNotifyClients_WhenNoGameFound()
    {
        Setup_GetDiscordTown(hasCachedTown: false, hasGuild: true, hasDayCategory: true, hasNightCategory: true, gameFound: false);

        _ = await _sut.GetDiscordTown(GuildId);

        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task GetDiscordTown_NotifiesClients_WhenGameFound()
    {
        Setup_GetDiscordTown(hasCachedTown: false, hasGuild: true, hasDayCategory: true, hasNightCategory: true, gameFound: true);

        _ = await _sut.GetDiscordTown(GuildId);

        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    #endregion

    #region GetDiscordTownDto

    private void Setup_GetDiscordTownDto(bool hasGamePerspective, bool hasGuild = true, bool hasUsers = false, string gameId = GameId, string? guildId = null, bool hasTown = false)
    {
        _discordTown = GetDummyDiscordTown();
        guildId ??= GuildId;
        var users = hasUsers
            ? new List<GameUser>
            {
                new(UserId)
            }
            : [];

        _mockGamePerspectiveStore.Setup(o => o.GetFirstPerspective(gameId)).Returns(hasGamePerspective ? CommonMethods.GetGamePerspective(GameId, guildId: guildId) with { Users = users } : null);
        _mockDiscordTownManager.Setup(o => o.GetDiscordTown(guildId)).Returns(hasTown ? _discordTown : null);
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(hasGuild ? new Mock<IDiscordGuild>().Object : null);
    }

    [TestMethod]
    public async Task GetDiscordTownDto_ReturnsFalse_WhenNoGameFound()
    {
        Setup_GetDiscordTownDto(false);

        var result = await _sut.GetDiscordTownDto(GameId);

        result.success.Should().BeFalse();
        result.discordTown.Should().BeNull();
        result.message.Should().Be($"Game not found for id: {GameId}");
    }

    [TestMethod]
    public async Task GetDiscordTownDto_ReturnsFalse_NoDiscordGuild()
    {
        Setup_GetDiscordTownDto(hasGamePerspective: true, hasGuild: false, hasTown: false);

        var result = await _sut.GetDiscordTownDto(GameId);

        result.success.Should().BeFalse();
        result.discordTown.Should().BeNull();
        result.message.Should().Be(GuildNotFoundMessage);
        _mockDiscordTownManager.Verify(o => o.GetDiscordTown(GuildId), Times.Once);
        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task GetDiscordTownDto_ReturnsTrue_WhenGotData(bool hasUsers)
    {
        Setup_GetDiscordTownDto(hasGamePerspective: true, hasGuild: true, hasUsers: hasUsers, hasTown: true);

        var result = await _sut.GetDiscordTownDto(GameId);

        result.success.Should().BeTrue();
        result.discordTown.Should().NotBeNull();
        result.discordTown.GameId.Should().Be(GameId);
        result.message.Should().Be("Got from store");
    }

    #endregion

    #region InviteUser

    private void Setup_InviteUser(bool hasPerspective = false, string? guildId = GuildId, bool hasGuild = false, bool hasUser = false, bool hasDmChannel = false)
    {
        _gameUser = CommonMethods.GetRandomGameUser(UserId);
        var gamePerspective = CommonMethods.GetGamePerspective(GameId, guildId: guildId);

        _mockGamePerspectiveStore.Setup(o => o.GetFirstPerspective(GameId)).Returns(hasPerspective ? gamePerspective : null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);

        _guild.Setup(o => o.Id).Returns(guildId!);
        _guild.Setup(o => o.GetUser(UserId)).Returns(hasUser ? _user.Object : null);

        _user.Setup(o => o.Id).Returns(UserId);
        _user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(hasDmChannel ? _dmChannel.Object : null);
        _user.Setup(o => o.AsGameUser(gamePerspective)).Returns(_gameUser);

        _mockJwtWriter.Setup(o => o.GetJwtToken(_gameUser)).Returns(Jwt);

        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(Key);

        _dmChannel.Setup(o => o.SendMessageAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        _mockCache.Setup(o => o.CreateEntry(It.IsAny<object>())).Returns(_cacheEntry.Object);
        _cacheEntry.SetupAllProperties();
        _cacheEntry.Setup(o => o.Dispose());
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenExceptionThrown()
    {
        _mockGamePerspectiveStore.Setup(o => o.GetFirstPerspective(GameId)).Throws<Exception>();

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        outcome.Should().Be(InviteUserOutcome.UnknownError);
        message.Should().Be("Failed to send message to user");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGameNotFound()
    {
        Setup_InviteUser(hasPerspective: false);

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        outcome.Should().Be(InviteUserOutcome.GameDoesNotExistError);
        message.Should().Be($"Couldn't find game with id: {GameId}");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("invalid-_guild")]
    public async Task InviteUser_ReturnsError_WhenGuildIdInvalid(string? guildId)
    {
        Setup_InviteUser(hasPerspective: true, guildId: guildId);

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        outcome.Should().Be(InviteUserOutcome.InvalidGuildError);
        message.Should().Be("GamePerspective contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGuildNotFound()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: false);

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        outcome.Should().Be(InviteUserOutcome.InvalidGuildError);
        message.Should().Be("GamePerspective contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenUserNotFound()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: false);

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        _guild.Verify(o => o.GetUser(UserId), Times.Once);
        outcome.Should().Be(InviteUserOutcome.UserNotFoundError);
        message.Should().Be($"Couldn't find user: {UserId}");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenDmChannelErrors()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: true, hasDmChannel: false);

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        _user.Verify(o => o.CreateDmChannelAsync(), Times.Once);
        outcome.Should().Be(InviteUserOutcome.DmChannelError);
        message.Should().Be("Couldn't open dm channel with user");
    }

    [TestMethod]
    public async Task InviteUser_GetsJwtToken_WhenDataGood()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: true, hasDmChannel: true);

        _ = await _sut.InviteUser(GameId, UserId, true);

        _mockJwtWriter.Verify(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId && g.UserType == UserType.Player)), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_GeneratesId_WhenGotJwt()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: true, hasDmChannel: true);

        _ = await _sut.InviteUser(GameId, UserId, true);

        _mockIdGenerator.Verify(o => o.GenerateId(), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_SetsCache_WhenGotKey()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: true, hasDmChannel: true);

        _ = await _sut.InviteUser(GameId, UserId, true);

        _mockCache.Verify(o => o.CreateEntry(It.Is<object>(e => (string)e == $"join_data_{Key}")), Times.Once);
        _cacheEntry.VerifySet(entry => entry.Value = It.Is<JoinData>(u => u.Jwt == Jwt && u.User.Id == UserId), Times.Once);
        _cacheEntry.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_SendsInvite_WhenDataGood()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: true, hasDmChannel: true);

        var (outcome, message) = await _sut.InviteUser(GameId, UserId, true);

        _dmChannel.Verify(o => o.SendMessageAsync($"[Join here]({FeUrl + $"/join?key={Key}"})"), Times.Once);
        _mockGamePerspectiveStore.Verify(o => o.AddUserToGame(GameId, It.Is<GameUser>(g => g.Id == UserId && g.UserType == UserType.Player)), Times.Once);
        outcome.Should().Be(InviteUserOutcome.InviteSent);
        message.Should().Be("Sent message to user");
    }

    [TestMethod]
    public async Task InviteUser_AddsUser_WhenDataGood()
    {
        Setup_InviteUser(hasPerspective: true, hasGuild: true, hasUser: true, hasDmChannel: true);

        _ = await _sut.InviteUser(GameId, UserId, true);

        _mockGamePerspectiveStore.Verify(o => o.AddUserToGame(GameId, _gameUser), Times.Once);
    }

    #endregion


    private static DiscordTown GetDummyDiscordTown(bool hasDayCategory = false, bool hasNightCategory = false)
    {
        var channelCategories = new List<MiniCategory>();
        if (hasDayCategory) channelCategories.Add(DayCategory);
        if (hasNightCategory) channelCategories.Add(NightCategory);
        var discordTown = new DiscordTown(channelCategories);
        return discordTown;
    }

    private static readonly MiniCategory DayCategory = new("day-category", "Day Category", [
        new ChannelOccupants(new MiniChannel("day-channel-1", "D`ay Channel 1"), [
            CommonMethods.GetRandomTownUser()
        ]),
        new ChannelOccupants(new MiniChannel("day-channel-2", "Day Channel 2"), [
            CommonMethods.GetRandomTownUser(),
            CommonMethods.GetRandomTownUser()
        ])
    ]);

    private static readonly MiniCategory NightCategory = new("night-category", "Night Category", [
        new ChannelOccupants(new MiniChannel("night-channel-1", "Night Channel 1"), []),
        new ChannelOccupants(new MiniChannel("night-channel-2", "Night Channel 2"), [
            CommonMethods.GetRandomTownUser()
        ]),
        new ChannelOccupants(new MiniChannel("night-channel-3", "Night Channel 3"), []),
    ]);
}