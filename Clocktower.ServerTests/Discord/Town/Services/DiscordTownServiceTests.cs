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
    private const ulong GuildId = 1L;
    private const ulong UserId = 2L;
    private const ulong ChannelId = 3L;
    private const string GameId = "this-game";
    private const string Jwt = "jwt-token";
    private const string Key = "this-key";
    private const string DayCategoryName = "dayCategoryName";
    private const string NightCategoryName = "nightCategoryName";
    private const string ExceptionMessage = "message";
    private const string GuildNotFoundMessage = "Guild not found";
    private const string StoryTellerRoleName = "StoryTellerRoleName";
    private static readonly string[] DayChannelNames = ["day-1", "day-2", "day-3"];
    private static readonly string[] NightChannelNames = ["night-1", "night-2", "night-3"];

    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<IDiscordTownStore> _mockDiscordTownStore = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private Mock<IMemoryCache> _mockCache = null!;
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private Mock<IDiscordConstantsService> _mockDiscordConstants = null!;
    private Mock<IIdGenerator> _mockIdGenerator = null!;

    private IDiscordTownService Sut => new DiscordTownService(
        _mockBot.Object,
        _mockNotificationService.Object,
        _mockGameStateStore.Object,
        _mockDiscordTownStore.Object,
        _mockJwtWriter.Object,
        _mockCache.Object,
        _mockSecrets.Object,
        _mockDiscordConstants.Object,
        _mockIdGenerator.Object
    );

    [TestInitialize]
    public void Setup()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockDiscordTownStore = new Mock<IDiscordTownStore>();
        _mockJwtWriter = new Mock<IJwtWriter>();
        _mockCache = new Mock<IMemoryCache>();
        _mockSecrets = new Mock<IOptions<Secrets>>();
        _mockDiscordConstants = new Mock<IDiscordConstantsService>();
        _mockIdGenerator = new Mock<IIdGenerator>();
    }


    #region MoveUser

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenGuildNotFound()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var (success, message) = await Sut.MoveUser(GuildId, UserId, ChannelId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenUserNotFound()
    {
        var guild = new Mock<IDiscordGuild>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns((IDiscordGuildUser)null!);

        var (success, message) = await Sut.MoveUser(GuildId, UserId, ChannelId);

        guild.Verify(o => o.GetUser(UserId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be("User not found in guild");
    }

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenUserNotInVoice()
    {
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.VoiceState).Returns((IDiscordVoiceState?)null);

        var (success, message) = await Sut.MoveUser(GuildId, UserId, ChannelId);

        success.Should().BeFalse();
        message.Should().Be("User is not connected to voice");
    }


    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenChannelNotFound()
    {
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var voiceState = new Mock<IDiscordVoiceState>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.VoiceState).Returns(voiceState.Object);
        guild.Setup(o => o.GetVoiceChannel(UserId)).Returns((IDiscordVoiceChannel)null!);

        var (success, message) = await Sut.MoveUser(GuildId, UserId, ChannelId);

        guild.Verify(o => o.GetVoiceChannel(ChannelId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be("Channel not found in guild");
    }


    [TestMethod]
    public async Task MoveUser_ReturnsTrueMovesUser_WhenDataGood()
    {
        const string userName = "userName";
        const string channelName = "channelName";
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var voiceState = new Mock<IDiscordVoiceState>();
        var channel = new Mock<IDiscordVoiceChannel>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.VoiceState).Returns(voiceState.Object);
        guild.Setup(o => o.GetVoiceChannel(ChannelId)).Returns(channel.Object);
        user.Setup(o => o.DisplayName).Returns(userName);
        channel.Setup(o => o.Name).Returns(channelName);

        var (success, message) = await Sut.MoveUser(GuildId, UserId, ChannelId);

        user.Verify(o => o.MoveAsync(channel.Object), Times.Once);
        success.Should().BeTrue();
        message.Should().Be($"User {userName} moved to {channelName}");
    }

    [TestMethod]
    public async Task MoveUser_ReturnsFalse_WhenExceptionThrown()
    {
        const string exceptionMessage = "something went wrong";
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(exceptionMessage));

        var (success, message) = await Sut.MoveUser(GuildId, UserId, ChannelId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        success.Should().BeFalse();
        message.Should().Be(exceptionMessage);
    }

    #endregion

    #region ToggleStoryTeller

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenNoGameFound()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns((GameState?)null);

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        _mockGameStateStore.Verify(o => o.Get(GameId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be($"Couldn't find game with id: {GameId}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenGuildIdInvalid()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = "invalid-guild" });

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        result.success.Should().BeFalse();
        result.message.Should().Be("GameState contained a guildId that is not valid");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenGuildNotFound()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = GuildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenRoleNotFound()
    {
        var guild = new Mock<IDiscordGuild>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = GuildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns((IDiscordRole?)null);

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        guild.Verify(o => o.GetRole(StoryTellerRoleName), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be($"{StoryTellerRoleName} role does not exist");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenUserNotFound()
    {
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = GuildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns((IDiscordGuildUser?)null);

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        guild.Verify(o => o.GetUser(UserId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("User not found");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_CallsDoesUserHaveRole_WhenDataGood()
    {
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = GuildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Roles).Returns([]);

        _ = await Sut.ToggleStoryTeller(GameId, UserId);

        user.Verify(o => o.DoesUserHaveRole(roleId), Times.Once);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_AddsUserToGame_WhenUserNotInGameState()
    {
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = GuildId.ToString(),
            Users = []
        };
        var gameUser = CommonMethods.GetRandomGameUser(UserId.ToString());
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.Roles).Returns([]);
        user.Setup(o => o.AsGameUser()).Returns(gameUser);

        _ = await Sut.ToggleStoryTeller(GameId, UserId);

        _mockGameStateStore.Verify(o => o.Get(GameId), Times.Exactly(2));
        _mockGameStateStore.Verify(o => o.AddUserToGame(GameId, gameUser), Times.Once);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_DoesNotUserToGame_WhenUserIsInGameState()
    {
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = GuildId.ToString(),
            Users =
            [
                CommonMethods.GetRandomGameUser(UserId.ToString())
            ]
        };
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.Roles).Returns([]);

        _ = await Sut.ToggleStoryTeller(GameId, UserId);

        _mockGameStateStore.Verify(o => o.Get(GameId), Times.Exactly(2));
        _mockGameStateStore.Verify(o => o.AddUserToGame(It.IsAny<string>(), It.IsAny<GameUser>()), Times.Never);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_AddsStoryTellerRole_WhenUserIsNotStoryTeller()
    {
        const string displayName = "displayName";
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = GuildId.ToString(),
            Users =
            [
                CommonMethods.GetRandomGameUser(UserId.ToString())
            ]
        };

        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.DisplayName).Returns(displayName);
        user.Setup(o => o.DoesUserHaveRole(roleId)).Returns(false);

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        user.Verify(o => o.AddRoleAsync(role.Object), Times.Once);
        _mockGameStateStore.Verify(o => o.UpdateUser(GameId, UserId, UserType.StoryTeller), Times.Once);

        result.success.Should().BeTrue();
        result.message.Should().Be($"User {displayName} is now a {StoryTellerRoleName}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_RemovesStoryTellerRole_WhenUserIsStoryTeller()
    {
        const string displayName = "displayName";
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = GuildId.ToString(),
            Users =
            [
                CommonMethods.GetRandomGameUser(UserId.ToString())
            ]
        };

        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.DisplayName).Returns(displayName);
        user.Setup(o => o.DoesUserHaveRole(roleId)).Returns(true);

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

        user.Verify(o => o.RemoveRoleAsync(role.Object), Times.Once);
        _mockGameStateStore.Verify(o => o.UpdateUser(GameId, UserId, UserType.Spectator), Times.Once);

        result.success.Should().BeTrue();
        result.message.Should().Be($"User {displayName} is no longer a {StoryTellerRoleName}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenExceptionThrown()
    {
        const string exceptionMessage = "exceptionMessage";
        _mockGameStateStore.Setup(o => o.Get(GameId)).Throws(new Exception(exceptionMessage));

        var result = await Sut.ToggleStoryTeller(GameId, UserId);

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

        var result = Sut.GetJoinData(Key);

        _mockCache.Verify(o => o.TryGetValue($"join_data_{Key}", out It.Ref<object?>.IsAny), Times.Once);
        result.Should().BeNull();
    }


    [TestMethod]
    public void GetJoinData_ReturnsNull_WhenCacheValueInvalid()
    {
        object invalidValue = "not-a-user-join-data-object";
        _mockCache.Setup(o => o.TryGetValue($"join_data_{Key}", out invalidValue!)).Returns(true);

        var result = Sut.GetJoinData(Key);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetJoinData_CallsCacheRemove_UpdatesGame_ReturnsData_WhenCacheValueFound()
    {
        var joinData = new JoinData(CommonMethods.GetRandomString(), CommonMethods.GetRandomGameUser(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString());
        object joinDataObj = joinData;
        _mockCache.Setup(o => o.TryGetValue($"join_data_{Key}", out joinDataObj!)).Returns(true);

        var result = Sut.GetJoinData(Key);

        _mockGameStateStore.Verify(o => o.UpdateUser(joinData.GameId, ulong.Parse(joinData.User.Id), null, true), Times.Once());
        _mockCache.Verify(o => o.Remove($"join_data_{Key}"), Times.Once);
        result.Should().Be(joinData);
    }

    #endregion

    #region PingUser

    [TestMethod]
    public async Task PingUser_PingsUser()
    {
        await Sut.PingUser(UserId.ToString());

        _mockNotificationService.Verify(o => o.PingUser(UserId.ToString(), "Ping!"), Times.Once);
    }

    #endregion

    #region DeleteTown

    [TestMethod]
    public async Task DeleteTown_ReturnsFalse_WhenGuildNotFound()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var result = await Sut.DeleteTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsTrue_WhenGuildFound()
    {
        var guild = new Mock<IDiscordGuild>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        var nightCategory = new Mock<IDiscordCategoryChannel>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(dayCategory.Object);
        guild.Setup(o => o.GetCategoryChannelByName(NightCategoryName)).Returns(nightCategory.Object);

        var result = await Sut.DeleteTown(GuildId);

        guild.Verify(o => o.GetCategoryChannelByName(DayCategoryName), Times.Once);
        dayCategory.Verify(o => o.DeleteAsync(), Times.Once);
        guild.Verify(o => o.GetCategoryChannelByName(NightCategoryName), Times.Once);
        nightCategory.Verify(o => o.DeleteAsync(), Times.Once);
        guild.Verify(o => o.DeleteRoleAsync(StoryTellerRoleName), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be("Town deleted");
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsFalse_WhenExceptionThrown()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(ExceptionMessage));

        var result = await Sut.DeleteTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(ExceptionMessage);
    }

    #endregion

    #region CreateTown

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenGuildNotFound()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var result = await Sut.CreateTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenExceptionThrown()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(ExceptionMessage));

        var result = await Sut.CreateTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(ExceptionMessage);
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenRoleNotCreated()
    {
        var roleColor = Color.Gold;
        var guild = new Mock<IDiscordGuild>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync((IDiscordRole)null!);

        var result = await Sut.CreateTown(GuildId);

        guild.Verify(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to create role");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenDayChannelsNotCreated()
    {
        var roleColor = Color.Gold;
        const ulong dayCategoryId = 10L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordRestCategoryChannel>();
        dayCategory.Setup(o => o.Id).Returns(dayCategoryId);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync(role.Object);
        guild.Setup(o => o.CreateCategoryAsync(DayCategoryName, everyoneCanSee: true)).ReturnsAsync(dayCategory.Object);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(DayChannelNames, dayCategoryId)).ReturnsAsync(false);

        var result = await Sut.CreateTown(GuildId);

        guild.Verify(o => o.CreateCategoryAsync(DayCategoryName, everyoneCanSee: true), Times.Once);
        guild.Verify(o => o.CreateVoiceChannelsForCategoryAsync(DayChannelNames, dayCategoryId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to generate day channels");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenNightChannelsNotCreated()
    {
        var roleColor = Color.Gold;
        const ulong dayCategoryId = 10L;
        const ulong nightCategoryId = 20L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordRestCategoryChannel>();
        var nightCategory = new Mock<IDiscordRestCategoryChannel>();
        dayCategory.Setup(o => o.Id).Returns(dayCategoryId);
        nightCategory.Setup(o => o.Id).Returns(nightCategoryId);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(NightChannelNames);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync(role.Object);
        guild.Setup(o => o.CreateCategoryAsync(DayCategoryName, everyoneCanSee: true)).ReturnsAsync(dayCategory.Object);
        guild.Setup(o => o.CreateCategoryAsync(NightCategoryName, everyoneCanSee: false, role.Object)).ReturnsAsync(nightCategory.Object);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(DayChannelNames, dayCategoryId)).ReturnsAsync(true);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(NightChannelNames, nightCategoryId)).ReturnsAsync(false);

        var result = await Sut.CreateTown(GuildId);

        guild.Verify(o => o.CreateCategoryAsync(NightCategoryName, everyoneCanSee: false, role.Object), Times.Once);
        guild.Verify(o => o.CreateVoiceChannelsForCategoryAsync(NightChannelNames, nightCategoryId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to generate night channels");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsTrue_WhenChannelsCreated()
    {
        var roleColor = Color.Gold;
        const ulong dayCategoryId = 10L;
        const ulong nightCategoryId = 20L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordRestCategoryChannel>();
        var nightCategory = new Mock<IDiscordRestCategoryChannel>();
        dayCategory.Setup(o => o.Id).Returns(dayCategoryId);
        nightCategory.Setup(o => o.Id).Returns(nightCategoryId);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(NightChannelNames);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync(role.Object);
        guild.Setup(o => o.CreateCategoryAsync(DayCategoryName, everyoneCanSee: true)).ReturnsAsync(dayCategory.Object);
        guild.Setup(o => o.CreateCategoryAsync(NightCategoryName, everyoneCanSee: false, role.Object)).ReturnsAsync(nightCategory.Object);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(DayChannelNames, dayCategoryId)).ReturnsAsync(true);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(NightChannelNames, nightCategoryId)).ReturnsAsync(true);

        var result = await Sut.CreateTown(GuildId);

        result.success.Should().BeTrue();
        result.message.Should().Be("Town created");
    }

    #endregion

    #region GetTownStatus

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenExceptionThrown()
    {
        const string exMessage = "message";
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(exMessage));

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeFalse();
        exists.Should().BeFalse();
        message.Should().Be(exMessage);
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenGuildNotFound()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeFalse();
        exists.Should().BeFalse();
        message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenStoryTellerRoleNotFound()
    {
        var guild = new Mock<IDiscordGuild>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns((IDiscordRole?)null);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be($"{StoryTellerRoleName} role does not exist");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenDayCategoryMissing()
    {
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns((IDiscordCategoryChannel?)null);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing day category");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenDayChannelsNotMatch()
    {
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(DayChannelNames)).Returns(false);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing day channels");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenNightCategoryMissing()
    {
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(DayChannelNames)).Returns(true);
        guild.Setup(o => o.GetCategoryChannelByName(NightCategoryName)).Returns((IDiscordCategoryChannel?)null);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing night category");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenNightChannelsNotMatch()
    {
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        var nightCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(NightChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(DayChannelNames)).Returns(true);
        guild.Setup(o => o.GetCategoryChannelByName(NightCategoryName)).Returns(nightCategory.Object);
        nightCategory.Setup(o => o.VerifyCategoryChannels(NightChannelNames)).Returns(false);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Not enough cottages");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsTrue_WhenTownStructureCorrect()
    {
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        var nightCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(DayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(NightChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(DayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(DayChannelNames)).Returns(true);
        guild.Setup(o => o.GetCategoryChannelByName(NightCategoryName)).Returns(nightCategory.Object);
        nightCategory.Setup(o => o.VerifyCategoryChannels(NightChannelNames)).Returns(true);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeTrue();
        message.Should().Be("Town structure is correct");
    }

    #endregion

    #region GetDiscordTown

    [TestMethod]
    public async Task GetDiscordTown_ReturnsFalse_WhenExceptionThrown()
    {
        const string exMessage = "message";
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Throws(new Exception(exMessage));

        var (success, discordTown, message) = await Sut.GetDiscordTown(GuildId);

        success.Should().BeFalse();
        discordTown.Should().BeNull();
        message.Should().Be(exMessage);
    }

    [TestMethod]
    public async Task GetDiscordTown_ReturnsTrue_WhenStoreHasValue()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(dummyDiscordTown);

        var (success, discordTown, message) = await Sut.GetDiscordTown(GuildId);

        success.Should().BeTrue();
        discordTown.Should().Be(dummyDiscordTown);
        message.Should().Be("Got from store");
    }

    [TestMethod]
    public async Task GetDiscordTown_ReturnsFalse_WhenGuildNotFound()
    {
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns((DiscordTown?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var (success, discordTown, message) = await Sut.GetDiscordTown(GuildId);

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
        var guild = new Mock<IDiscordGuild>();
        var channelCategories = new List<MiniCategory>();
        if (hasDayCategory) channelCategories.Add(DayCategory);
        if (hasNightCategory) channelCategories.Add(NightCategory);
        var expectedDiscordTown = new DiscordTown(channelCategories);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns((DiscordTown?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(hasDayCategory ? DayCategory : null);
        guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(hasNightCategory ? NightCategory : null);

        var (success, discordTown, message) = await Sut.GetDiscordTown(GuildId);

        success.Should().BeTrue();
        discordTown.Should().BeEquivalentTo(expectedDiscordTown);
        message.Should().Be($"Discord town {discordTown.UserCount}");
    }

    [TestMethod]
    public async Task GetDiscordTown_NotifiesClients_WhenGameFound()
    {
        var guild = new Mock<IDiscordGuild>();
        var expectedDiscordTown = GetDummyDiscordTown();
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns((DiscordTown?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(DayCategory);
        guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(NightCategory);
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([new GameState { Id = GameId }]);

        DiscordTown? capturedDiscordTown = null;
        _mockNotificationService.Setup(o =>
            o.BroadcastDiscordTownUpdate(It.IsAny<string>(), It.IsAny<DiscordTown>())
        ).Callback<string, DiscordTown>((_, town) => capturedDiscordTown = town);

        _ = await Sut.GetDiscordTown(GuildId);

        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(GameId, It.IsAny<DiscordTown>()), Times.Once);
        capturedDiscordTown.Should().BeEquivalentTo(expectedDiscordTown);
    }

    [TestMethod]
    public async Task GetDiscordTown_DoesNotNotifyClients_WhenNoGameFound()
    {
        var guild = new Mock<IDiscordGuild>();
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns((DiscordTown?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(DayCategory);
        guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(NightCategory);
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([]);

        _ = await Sut.GetDiscordTown(GuildId);

        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(GameId, It.IsAny<DiscordTown>()), Times.Never);
    }

    #endregion

    #region GetDiscordTownDto

    private void Setup_GetDiscordTownDto(bool hasGameState, bool hasGuild = true, bool hasUsers = false, string gameId = GameId, ulong guildId = GuildId, DiscordTown? discordTown = null)
    {
        var users = hasUsers
            ? new List<GameUser>
            {
                new(UserId.ToString())
            }
            : [];

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(hasGameState ? new GameState { Id = gameId, GuildId = guildId.ToString(), Users = users } : null);
        _mockDiscordTownStore.Setup(o => o.Get(GuildId)).Returns(discordTown);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? new Mock<IDiscordGuild>().Object : null);
    }

    [TestMethod]
    public async Task GetDiscordTownDto_ReturnsFalse_WhenNoGameFound()
    {
        Setup_GetDiscordTownDto(false);

        var result = await Sut.GetDiscordTownDto(GameId);

        result.success.Should().BeFalse();
        result.discordTown.Should().BeNull();
        result.message.Should().Be($"Game not found for id: {GameId}");
    }

    [TestMethod]
    public async Task GetDiscordTownDto_ReturnsFalse_NoDiscordGuild()
    {
        Setup_GetDiscordTownDto(hasGameState: true, hasGuild: false, discordTown: null);

        var result = await Sut.GetDiscordTownDto(GameId);

        result.success.Should().BeFalse();
        result.discordTown.Should().BeNull();
        result.message.Should().Be(GuildNotFoundMessage);
        _mockDiscordTownStore.Verify(o => o.Get(GuildId), Times.Once);
        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task GetDiscordTownDto_ReturnsTrue_WhenGotData(bool hasUsers)
    {
        var discordTown = GetDummyDiscordTown();
        Setup_GetDiscordTownDto(hasGameState: true, hasGuild: true, hasUsers: hasUsers, discordTown: discordTown);

        var result = await Sut.GetDiscordTownDto(GameId);

        result.success.Should().BeTrue();
        result.discordTown.Should().NotBeNull();
        result.discordTown.GameId.Should().Be(GameId);
        result.message.Should().Be("Got from store");
    }

    #endregion

    #region InviteUser

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenExceptionThrown()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Throws<Exception>();

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        outcome.Should().Be(InviteUserOutcome.UnknownError);
        message.Should().Be("Failed to send message to user");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGameNotFound()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns((GameState?)null);

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        outcome.Should().Be(InviteUserOutcome.GameDoesNotExistError);
        message.Should().Be($"Couldn't find game with id: {GameId}");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGuildIdInvalid()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = "invalid-guild" });

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        outcome.Should().Be(InviteUserOutcome.InvalidGuildError);
        message.Should().Be("GameState contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGuildNotFound()
    {
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        outcome.Should().Be(InviteUserOutcome.InvalidGuildError);
        message.Should().Be("GameState contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenUserNotFound()
    {
        var guild = new Mock<IDiscordGuild>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns((IDiscordGuildUser?)null);

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        guild.Verify(o => o.GetUser(UserId), Times.Once);
        outcome.Should().Be(InviteUserOutcome.UserNotFoundError);
        message.Should().Be($"Couldn't find user: {UserId}");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenDmChannelErrors()
    {
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync((IDiscordDmChannel?)null);

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        user.Verify(o => o.CreateDmChannelAsync(), Times.Once);
        outcome.Should().Be(InviteUserOutcome.DmChannelError);
        message.Should().Be("Couldn't open dm channel with user");
    }

    [TestMethod]
    public async Task InviteUser_GetsJwtToken_WhenDataGood()
    {
        var gameState = new GameState { GuildId = "1" };
        var gameUser = CommonMethods.GetRandomGameUser(UserId.ToString());
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        user.Setup(o => o.AsGameUser(gameState)).Returns(gameUser);
        _mockJwtWriter.Setup(o => o.GetJwtToken(gameUser)).Returns(Jwt);

        _ = await Sut.InviteUser(GameId, UserId);

        _mockJwtWriter.Verify(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player)), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_GeneratesId_WhenGotJwt()
    {
        var gameState = new GameState { GuildId = "1" };
        var gameUser = CommonMethods.GetRandomGameUser(UserId.ToString());
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        user.Setup(o => o.AsGameUser(gameState)).Returns(gameUser);
        _mockJwtWriter.Setup(o => o.GetJwtToken(gameUser)).Returns(Jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(Key);

        _ = await Sut.InviteUser(GameId, UserId);

        _mockIdGenerator.Verify(o => o.GenerateId(), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_SetsCache_WhenGotKey()
    {
        const string feUri = "fe-uri";
        CommonMethods.SetUpMockSecrets(_mockSecrets, feUri: feUri);
        var gameState = new GameState { GuildId = "1" };
        var gameUser = CommonMethods.GetRandomGameUser(UserId.ToString());
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        user.Setup(o => o.AsGameUser(gameState)).Returns(gameUser);
        _mockJwtWriter.Setup(o => o.GetJwtToken(gameUser)).Returns(Jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(Key);
        var mockCacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(o => o.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        _ = await Sut.InviteUser(GameId, UserId);

        _mockCache.Verify(o => o.CreateEntry(It.Is<object>(e => e.ToString() == $"join_data_{Key}")), Times.Once);
        mockCacheEntry.VerifySet(entry => entry.Value = It.Is<JoinData>(u => u.Jwt == Jwt && u.User.Id == UserId.ToString()), Times.Once);
        mockCacheEntry.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_SendsInvite_WhenDataGood()
    {
        const string feUri = "fe-uri";
        CommonMethods.SetUpMockSecrets(_mockSecrets, feUri: feUri);
        var gameState = new GameState { GuildId = "1" };
        var gameUser = CommonMethods.GetRandomGameUser(UserId.ToString());
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        user.Setup(o => o.AsGameUser(gameState)).Returns(gameUser);
        _mockJwtWriter.Setup(o => o.GetJwtToken(gameUser)).Returns(Jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(Key);
        var mockCacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(o => o.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        var (outcome, message) = await Sut.InviteUser(GameId, UserId);

        dmChannel.Verify(o => o.SendMessageAsync($"[Join here]({feUri + $"/join?key={Key}"})"), Times.Once);
        _mockGameStateStore.Verify(o => o.AddUserToGame(GameId, It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player)), Times.Once);
        outcome.Should().Be(InviteUserOutcome.InviteSent);
        message.Should().Be("Sent message to user");
    }

    #endregion

    private static DiscordTown GetDummyDiscordTown()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var discordTown = new DiscordTown(channelCategories);
        return discordTown;
    }

    private static readonly MiniCategory DayCategory = new("day-category", "Day Category", [
        new ChannelOccupants(new MiniChannel("day-channel-1", "Day Channel 1"), [
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