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
using static Clocktower.ServerTests.TestHelpers.TestDataProvider;

namespace Clocktower.ServerTests.Discord.Town.Services;

[TestClass]
public class DiscordTownServiceTests
{
    private const ulong GuildId = 1L;
    private const ulong UserId = 2L;
    private const ulong ChannelId = 3L;
    private const string GuildNotFoundMessage = "Guild not found";
    private const string StoryTellerRoleName = "StoryTellerRoleName";

    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<ITownOccupancyStore> _mockTownOccupancyStore = null!;
    private Mock<IJwtWriter> _mockJwtWriter = null!;
    private Mock<IMemoryCache> _mockCache = null!;
    private Mock<IOptions<Secrets>> _mockSecrets = null!;
    private Mock<IDiscordConstantsService> _mockDiscordConstants = null!;
    private Mock<IIdGenerator> _mockIdGenerator = null!;

    private IDiscordTownService Sut => new DiscordTownService(
        _mockBot.Object,
        _mockNotificationService.Object,
        _mockGameStateStore.Object,
        _mockTownOccupancyStore.Object,
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
        _mockTownOccupancyStore = new Mock<ITownOccupancyStore>();
        _mockJwtWriter = new Mock<IJwtWriter>();
        _mockCache = new Mock<IMemoryCache>();
        _mockSecrets = new Mock<IOptions<Secrets>>();
        _mockDiscordConstants = new Mock<IDiscordConstantsService>();
        _mockIdGenerator = new Mock<IIdGenerator>();
    }


    private void SetUpSecrets(
        string? discordBotToken = null,
        string? discordClientId = null,
        string? discordClientSecret = null,
        string? serverUri = null,
        string? feUri = null,
        string? jwtSigningKey = null,
        string? jwtAudience = null
    )
    {
        var secrets = new Secrets
        {
            DiscordBotToken = discordBotToken!,
            DiscordClientId = discordClientId!,
            DiscordClientSecret = discordClientSecret!,
            ServerUri = serverUri!,
            FeUri = feUri!,
            Jwt = new JwtSecrets
            {
                SigningKey = jwtSigningKey!,
                Audience = jwtAudience!
            }
        };
        _mockSecrets.Setup(o => o.Value).Returns(secrets);
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
        const string gameId = "this-game";
        const ulong userId = 1L;
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns((GameState?)null);

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        _mockGameStateStore.Verify(o => o.Get(gameId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be($"Couldn't find game with id: {gameId}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenGuildIdInvalid()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "invalid-guild" });

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        result.success.Should().BeFalse();
        result.message.Should().Be("GameState contained a guildId that is not valid");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenGuildNotFound()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = guildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns((IDiscordGuild?)null);

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        _mockBot.Verify(o => o.GetGuild(guildId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be(GuildNotFoundMessage);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenRoleNotFound()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        var guild = new Mock<IDiscordGuild>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = guildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns((IDiscordRole?)null);

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        guild.Verify(o => o.GetRole(StoryTellerRoleName), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be($"{StoryTellerRoleName} role does not exist");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenUserNotFound()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = guildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(userId)).Returns((IDiscordGuildUser?)null);

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        guild.Verify(o => o.GetUser(userId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("User not found");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_CallsDoesUserHaveRole_WhenDataGood()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = guildId.ToString() });
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(userId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Roles).Returns([]);

        _ = await Sut.ToggleStoryTeller(gameId, userId);

        user.Verify(o => o.DoesUserHaveRole(roleId), Times.Once);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_AddsUserToGame_WhenUserNotInGameState()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = guildId.ToString(),
            Users = []
        };
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(userId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(userId);
        user.Setup(o => o.Roles).Returns([]);

        _ = await Sut.ToggleStoryTeller(gameId, userId);

        _mockGameStateStore.Verify(o => o.Get(gameId), Times.Exactly(2));
        _mockGameStateStore.Verify(o => o.AddUserToGame(gameId, It.Is<GameUser>(g => g.Id == userId.ToString())), Times.Once);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_DoesNotUserToGame_WhenUserIsInGameState()
    {
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = guildId.ToString(),
            Users =
            [
                new GameUser(userId.ToString(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString())
            ]
        };
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(userId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(userId);
        user.Setup(o => o.Roles).Returns([]);

        _ = await Sut.ToggleStoryTeller(gameId, userId);

        _mockGameStateStore.Verify(o => o.Get(gameId), Times.Exactly(2));
        _mockGameStateStore.Verify(o => o.AddUserToGame(It.IsAny<string>(), It.IsAny<GameUser>()), Times.Never);
    }

    [TestMethod]
    public async Task ToggleStoryTeller_AddsStoryTellerRole_WhenUserIsNotStoryTeller()
    {
        const string displayName = "displayName";
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = guildId.ToString(),
            Users =
            [
                new GameUser(userId.ToString(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString())
            ]
        };

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(userId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(userId);
        user.Setup(o => o.DisplayName).Returns(displayName);
        user.Setup(o => o.DoesUserHaveRole(roleId)).Returns(false);

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        user.Verify(o => o.AddRoleAsync(role.Object), Times.Once);
        _mockGameStateStore.Verify(o => o.UpdateUser(gameId, userId, UserType.StoryTeller), Times.Once);

        result.success.Should().BeTrue();
        result.message.Should().Be($"User {displayName} is now a {StoryTellerRoleName}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_RemovesStoryTellerRole_WhenUserIsStoryTeller()
    {
        const string displayName = "displayName";
        const string gameId = "this-game";
        const ulong userId = 1L;
        const ulong guildId = 2L;
        const ulong roleId = 3L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var user = new Mock<IDiscordGuildUser>();
        var gameState = new GameState
        {
            GuildId = guildId.ToString(),
            Users =
            [
                new GameUser(userId.ToString(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString())
            ]
        };

        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(gameState);
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetUser(userId)).Returns(user.Object);
        role.Setup(o => o.Id).Returns(roleId);
        user.Setup(o => o.Id).Returns(userId);
        user.Setup(o => o.DisplayName).Returns(displayName);
        user.Setup(o => o.DoesUserHaveRole(roleId)).Returns(true);

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        user.Verify(o => o.RemoveRoleAsync(role.Object), Times.Once);
        _mockGameStateStore.Verify(o => o.UpdateUser(gameId, userId, UserType.Spectator), Times.Once);

        result.success.Should().BeTrue();
        result.message.Should().Be($"User {displayName} is no longer a {StoryTellerRoleName}");
    }

    [TestMethod]
    public async Task ToggleStoryTeller_ReturnsFalse_WhenExceptionThrown()
    {
        const string exceptionMessage = "exceptionMessage";
        const string gameId = "this-game";
        const ulong userId = 1L;
        _mockGameStateStore.Setup(o => o.Get(gameId)).Throws(new Exception(exceptionMessage));

        var result = await Sut.ToggleStoryTeller(gameId, userId);

        result.success.Should().BeFalse();
        result.message.Should().Be(exceptionMessage);
    }

    #endregion

    #region GetJoinData

    [TestMethod]
    public void GetJoinData_ReturnsNull_WhenKeyNotFound()
    {
        const string key = "this-key";
        object invalidValue = null!;
        _mockCache.Setup(o => o.TryGetValue($"join_data_{key}", out invalidValue!)).Returns(false);

        var result = Sut.GetJoinData(key);

        _mockCache.Verify(o => o.TryGetValue($"join_data_{key}", out It.Ref<object?>.IsAny), Times.Once);
        result.Should().BeNull();
    }


    [TestMethod]
    public void GetJoinData_ReturnsNull_WhenCacheValueInvalid()
    {
        const string key = "this-key";
        object invalidValue = "not-a-user-join-data-object";
        _mockCache.Setup(o => o.TryGetValue($"join_data_{key}", out invalidValue!)).Returns(true);

        var result = Sut.GetJoinData(key);

        result.Should().BeNull();
    }

    [TestMethod]
    public void GetJoinData_CallsCacheRemove_UpdatesGame_ReturnsData_WhenCacheValueFound()
    {
        const string key = "this-key";
        var joinData = new JoinData(CommonMethods.GetRandomString(), CommonMethods.GetRandomGameUser(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString());
        object joinDataObj = joinData;
        _mockCache.Setup(o => o.TryGetValue($"join_data_{key}", out joinDataObj!)).Returns(true);

        var result = Sut.GetJoinData(key);

        _mockGameStateStore.Verify(o => o.UpdateUser(joinData.GameId, ulong.Parse(joinData.User.Id), null, true), Times.Once());
        _mockCache.Verify(o => o.Remove($"join_data_{key}"), Times.Once);
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

    #region SetTime

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenGameNotFound()
    {
        const string gameId = "game-id";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns((GameState?)null);

        var result = await Sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be("Game not found");
    }

    [TestMethod]
    [DynamicData(nameof(GetGameTimeValues))]
    public async Task SetTime_SetsTime_NotifyClients_WhenDataGood(GameTime gameTime)
    {
        const string gameId = "game-id";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = GuildId.ToString() });

        var result = await Sut.SetTime(gameId, gameTime);

        _mockGameStateStore.Verify(o => o.SetTime(gameId, gameTime), Times.Once);
        _mockNotificationService.Verify(o => o.BroadcastTownTime(gameId, gameTime), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be($"Time set to {gameTime}");
    }

    [TestMethod]
    public async Task SetTime_ReturnsFalse_WhenExceptionThrown()
    {
        const string gameId = "game-id";
        const string message = "message";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Throws(new Exception(message));

        var result = await Sut.SetTime(gameId, GameTime.Evening);

        result.success.Should().BeFalse();
        result.message.Should().Be(message);
    }

    #endregion

    #region DeleteTown

    [TestMethod]
    public async Task DeleteTown_ReturnsFalse_WhenGuildNotFound()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var result = await Sut.DeleteTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be("Guild not found");
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsTrue_WhenGuildFound()
    {
        const string dayCategoryName = "dayCategoryName";
        const string nightCategoryName = "nightCategoryName";
        var guild = new Mock<IDiscordGuild>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        var nightCategory = new Mock<IDiscordCategoryChannel>();
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(nightCategoryName);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.GetCategoryChannelByName(dayCategoryName)).Returns(dayCategory.Object);
        guild.Setup(o => o.GetCategoryChannelByName(nightCategoryName)).Returns(nightCategory.Object);

        var result = await Sut.DeleteTown(GuildId);

        guild.Verify(o => o.GetCategoryChannelByName(dayCategoryName), Times.Once);
        dayCategory.Verify(o => o.DeleteAsync(), Times.Once);
        guild.Verify(o => o.GetCategoryChannelByName(nightCategoryName), Times.Once);
        nightCategory.Verify(o => o.DeleteAsync(), Times.Once);
        guild.Verify(o => o.DeleteRoleAsync(StoryTellerRoleName), Times.Once);
        result.success.Should().BeTrue();
        result.message.Should().Be("Town deleted");
    }

    [TestMethod]
    public async Task DeleteTown_ReturnsFalse_WhenExceptionThrown()
    {
        const string message = "message";
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(message));

        var result = await Sut.DeleteTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(message);
    }

    #endregion

    #region CreateTown

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenGuildNotFound()
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var result = await Sut.CreateTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be("Guild not found");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenExceptionThrown()
    {
        const string message = "message";
        _mockBot.Setup(o => o.GetGuild(GuildId)).Throws(new Exception(message));

        var result = await Sut.CreateTown(GuildId);

        result.success.Should().BeFalse();
        result.message.Should().Be(message);
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
        const string dayCategoryName = "dayCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        const ulong dayCategoryId = 10L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordRestCategoryChannel>();
        dayCategory.Setup(o => o.Id).Returns(dayCategoryId);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync(role.Object);
        guild.Setup(o => o.CreateCategoryAsync(dayCategoryName, everyoneCanSee: true)).ReturnsAsync(dayCategory.Object);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(dayChannelNames, dayCategoryId)).ReturnsAsync(false);

        var result = await Sut.CreateTown(GuildId);

        guild.Verify(o => o.CreateCategoryAsync(dayCategoryName, everyoneCanSee: true), Times.Once);
        guild.Verify(o => o.CreateVoiceChannelsForCategoryAsync(dayChannelNames, dayCategoryId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to generate day channels");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsFalse_WhenNightChannelsNotCreated()
    {
        var roleColor = Color.Gold;
        const string dayCategoryName = "dayCategoryName";
        const string nightCategoryName = "nightCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var nightChannelNames = new[] { "night-1", "night-2", "night-3" };
        const ulong dayCategoryId = 10L;
        const ulong nightCategoryId = 20L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordRestCategoryChannel>();
        var nightCategory = new Mock<IDiscordRestCategoryChannel>();
        dayCategory.Setup(o => o.Id).Returns(dayCategoryId);
        nightCategory.Setup(o => o.Id).Returns(nightCategoryId);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(nightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(nightChannelNames);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync(role.Object);
        guild.Setup(o => o.CreateCategoryAsync(dayCategoryName, everyoneCanSee: true)).ReturnsAsync(dayCategory.Object);
        guild.Setup(o => o.CreateCategoryAsync(nightCategoryName, everyoneCanSee: false, role.Object)).ReturnsAsync(nightCategory.Object);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(dayChannelNames, dayCategoryId)).ReturnsAsync(true);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(nightChannelNames, nightCategoryId)).ReturnsAsync(false);

        var result = await Sut.CreateTown(GuildId);

        guild.Verify(o => o.CreateCategoryAsync(nightCategoryName, everyoneCanSee: false, role.Object), Times.Once);
        guild.Verify(o => o.CreateVoiceChannelsForCategoryAsync(nightChannelNames, nightCategoryId), Times.Once);
        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to generate night channels");
    }

    [TestMethod]
    public async Task CreateTown_ReturnsTrue_WhenChannelsCreated()
    {
        var roleColor = Color.Gold;
        const string dayCategoryName = "dayCategoryName";
        const string nightCategoryName = "nightCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var nightChannelNames = new[] { "night-1", "night-2", "night-3" };
        const ulong dayCategoryId = 10L;
        const ulong nightCategoryId = 20L;
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordRestCategoryChannel>();
        var nightCategory = new Mock<IDiscordRestCategoryChannel>();
        dayCategory.Setup(o => o.Id).Returns(dayCategoryId);
        nightCategory.Setup(o => o.Id).Returns(nightCategoryId);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(nightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(nightChannelNames);
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        guild.Setup(o => o.CreateRoleAsync(StoryTellerRoleName, roleColor)).ReturnsAsync(role.Object);
        guild.Setup(o => o.CreateCategoryAsync(dayCategoryName, everyoneCanSee: true)).ReturnsAsync(dayCategory.Object);
        guild.Setup(o => o.CreateCategoryAsync(nightCategoryName, everyoneCanSee: false, role.Object)).ReturnsAsync(nightCategory.Object);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(dayChannelNames, dayCategoryId)).ReturnsAsync(true);
        guild.Setup(o => o.CreateVoiceChannelsForCategoryAsync(nightChannelNames, nightCategoryId)).ReturnsAsync(true);

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
        message.Should().Be("Guild not found");
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
        const string dayCategoryName = "dayCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(dayCategoryName)).Returns((IDiscordCategoryChannel?)null);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing day category");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenDayChannelsNotMatch()
    {
        const string dayCategoryName = "dayCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(dayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(dayChannelNames)).Returns(false);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing day channels");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenNightCategoryMissing()
    {
        const string dayCategoryName = "dayCategoryName";
        const string nightCategoryName = "nightCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(nightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(dayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(dayChannelNames)).Returns(true);
        guild.Setup(o => o.GetCategoryChannelByName(nightCategoryName)).Returns((IDiscordCategoryChannel?)null);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Missing night category");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsFalse_WhenNightChannelsNotMatch()
    {
        const string dayCategoryName = "dayCategoryName";
        const string nightCategoryName = "nightCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var nightChannelNames = new[] { "night-1", "night-2", "night-3" };
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        var nightCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(nightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(nightChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(dayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(dayChannelNames)).Returns(true);
        guild.Setup(o => o.GetCategoryChannelByName(nightCategoryName)).Returns(nightCategory.Object);
        nightCategory.Setup(o => o.VerifyCategoryChannels(nightChannelNames)).Returns(false);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeFalse();
        message.Should().Be("Not enough cottages");
    }

    [TestMethod]
    public void GetTownStatus_ReturnsTrue_WhenTownStructureCorrect()
    {
        const string dayCategoryName = "dayCategoryName";
        const string nightCategoryName = "nightCategoryName";
        var dayChannelNames = new[] { "day-1", "day-2", "day-3" };
        var nightChannelNames = new[] { "night-1", "night-2", "night-3" };
        var guild = new Mock<IDiscordGuild>();
        var role = new Mock<IDiscordRole>();
        var dayCategory = new Mock<IDiscordCategoryChannel>();
        var nightCategory = new Mock<IDiscordCategoryChannel>();
        _mockDiscordConstants.Setup(o => o.StoryTellerRoleName).Returns(StoryTellerRoleName);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(dayCategoryName);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(nightCategoryName);
        _mockDiscordConstants.Setup(o => o.DayRoomNames).Returns(dayChannelNames);
        _mockDiscordConstants.Setup(o => o.GetNightRoomNames()).Returns(nightChannelNames);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetRole(StoryTellerRoleName)).Returns(role.Object);
        guild.Setup(o => o.GetCategoryChannelByName(dayCategoryName)).Returns(dayCategory.Object);
        dayCategory.Setup(o => o.VerifyCategoryChannels(dayChannelNames)).Returns(true);
        guild.Setup(o => o.GetCategoryChannelByName(nightCategoryName)).Returns(nightCategory.Object);
        nightCategory.Setup(o => o.VerifyCategoryChannels(nightChannelNames)).Returns(true);

        var (success, exists, message) = Sut.GetTownStatus(GuildId);

        success.Should().BeTrue();
        exists.Should().BeTrue();
        message.Should().Be("Town structure is correct");
    }

    #endregion

    #region GetTownOccupancy

    [TestMethod]
    public async Task GetTownOccupancy_ReturnsFalse_WhenExceptionThrown()
    {
        const string exMessage = "message";
        _mockTownOccupancyStore.Setup(o => o.Get(GuildId)).Throws(new Exception(exMessage));

        var (success, townOccupants, message) = await Sut.GetTownOccupancy(GuildId);

        success.Should().BeFalse();
        townOccupants.Should().BeNull();
        message.Should().Be(exMessage);
    }

    [TestMethod]
    public async Task GetTownOccupancy_ReturnsTrue_WhenStoreHasValue()
    {
        var dummyTownOccupants = GetDummyTownOccupants();
        _mockTownOccupancyStore.Setup(o => o.Get(GuildId)).Returns(dummyTownOccupants);

        var (success, townOccupants, message) = await Sut.GetTownOccupancy(GuildId);

        success.Should().BeTrue();
        townOccupants.Should().Be(dummyTownOccupants);
        message.Should().Be("Got from store");
    }

    [TestMethod]
    public async Task GetTownOccupancy_ReturnsFalse_WhenGuildNotFound()
    {
        _mockTownOccupancyStore.Setup(o => o.Get(GuildId)).Returns((TownOccupants?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var (success, townOccupants, message) = await Sut.GetTownOccupancy(GuildId);

        success.Should().BeFalse();
        townOccupants.Should().Be(null);
        message.Should().Be("Guild not found");
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(false, false)]
    public async Task GetTownOccupancy_SetsStore_WithExpectedCategories(bool hasDayCategory, bool hasNightCategory)
    {
        var guild = new Mock<IDiscordGuild>();
        var channelCategories = new List<MiniCategory>();
        if (hasDayCategory) channelCategories.Add(DayCategory);
        if (hasNightCategory) channelCategories.Add(NightCategory);
        var expectedTownOccupants = new TownOccupants(channelCategories);
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);
        _mockTownOccupancyStore.Setup(o => o.Get(GuildId)).Returns((TownOccupants?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(hasDayCategory ? DayCategory : null);
        guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(hasNightCategory ? NightCategory : null);

        var (success, townOccupants, message) = await Sut.GetTownOccupancy(GuildId);

        success.Should().BeTrue();
        townOccupants.Should().BeEquivalentTo(expectedTownOccupants);
        message.Should().Be($"Town occupancy {townOccupants.UserCount}");
    }

    [TestMethod]
    public async Task GetTownOccupancy_NotifiesClients_WhenGameFound()
    {
        const string gameId = "game-id";
        var guild = new Mock<IDiscordGuild>();
        var expectedTownOccupants = GetDummyTownOccupants();
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);
        _mockTownOccupancyStore.Setup(o => o.Get(GuildId)).Returns((TownOccupants?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(DayCategory);
        guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(NightCategory);
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([new GameState { Id = gameId }]);

        TownOccupants? capturedTownOccupants = null;
        _mockNotificationService.Setup(o =>
            o.BroadcastTownOccupancyUpdate(It.IsAny<string>(), It.IsAny<TownOccupants>())
        ).Callback<string, TownOccupants>((_, occupants) => capturedTownOccupants = occupants);

        _ = await Sut.GetTownOccupancy(GuildId);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(gameId, It.IsAny<TownOccupants>()), Times.Once);
        capturedTownOccupants.Should().BeEquivalentTo(expectedTownOccupants);
    }

    [TestMethod]
    public async Task GetTownOccupancy_DoesNotNotifyClients_WhenNoGameFound()
    {
        const string gameId = "game-id";
        var guild = new Mock<IDiscordGuild>();
        _mockDiscordConstants.Setup(o => o.DayCategoryName).Returns(DayCategory.Name);
        _mockDiscordConstants.Setup(o => o.NightCategoryName).Returns(NightCategory.Name);
        _mockTownOccupancyStore.Setup(o => o.Get(GuildId)).Returns((TownOccupants?)null);
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetMiniCategory(DayCategory.Name)).Returns(DayCategory);
        guild.Setup(o => o.GetMiniCategory(NightCategory.Name)).Returns(NightCategory);
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([]);

        _ = await Sut.GetTownOccupancy(GuildId);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(gameId, It.IsAny<TownOccupants>()), Times.Never);
    }

    #endregion

    #region InviteUser

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenExceptionThrown()
    {
        const string gameId = "game-id";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Throws<Exception>();

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        outcome.Should().Be(InviteUserOutcome.UnknownError);
        message.Should().Be("Failed to send message to user");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGameNotFound()
    {
        const string gameId = "game-id";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns((GameState?)null);

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        outcome.Should().Be(InviteUserOutcome.GameDoesNotExistError);
        message.Should().Be($"Couldn't find game with id: {gameId}");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGuildIdInvalid()
    {
        const string gameId = "this-game";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "invalid-guild" });

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        outcome.Should().Be(InviteUserOutcome.InvalidGuildError);
        message.Should().Be("GameState contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenGuildNotFound()
    {
        const string gameId = "this-game";
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns((IDiscordGuild?)null);

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        _mockBot.Verify(o => o.GetGuild(GuildId), Times.Once);
        outcome.Should().Be(InviteUserOutcome.InvalidGuildError);
        message.Should().Be("GameState contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenUserNotFound()
    {
        const string gameId = "this-game";
        var guild = new Mock<IDiscordGuild>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns((IDiscordGuildUser?)null);

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        guild.Verify(o => o.GetUser(UserId), Times.Once);
        outcome.Should().Be(InviteUserOutcome.UserNotFoundError);
        message.Should().Be($"Couldn't find user: {UserId}");
    }

    [TestMethod]
    public async Task InviteUser_ReturnsError_WhenDmChannelErrors()
    {
        const string gameId = "this-game";
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync((IDiscordDmChannel?)null);

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        user.Verify(o => o.CreateDmChannelAsync(), Times.Once);
        outcome.Should().Be(InviteUserOutcome.DmChannelError);
        message.Should().Be("Couldn't open dm channel with user");
    }

    [TestMethod]
    public async Task InviteUser_GetsJwtToken_WhenDataGood()
    {
        const string gameId = "this-game";
        const string jwt = "jwt-token";
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player))).Returns(jwt);

        _ = await Sut.InviteUser(gameId, UserId);

        _mockJwtWriter.Verify(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player)), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_GeneratesId_WhenGotJwt()
    {
        const string gameId = "this-game";
        const string jwt = "jwt-token";
        const string key = "this-key";
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player))).Returns(jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(key);

        _ = await Sut.InviteUser(gameId, UserId);

        _mockIdGenerator.Verify(o => o.GenerateId(), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_SetsCache_WhenGotKey()
    {
        const string gameId = "this-game";
        const string jwt = "jwt-token";
        const string key = "this-key";
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player))).Returns(jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(key);
        var mockCacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(o => o.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        _ = await Sut.InviteUser(gameId, UserId);

        _mockCache.Verify(o => o.CreateEntry(It.Is<object>(e => e.ToString() == $"join_data_{key}")), Times.Once);
        mockCacheEntry.VerifySet(entry => entry.Value = It.Is<JoinData>(u => u.Jwt == jwt && u.User.Id == UserId.ToString()), Times.Once);
        mockCacheEntry.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), Times.Once);
    }

    [TestMethod]
    public async Task InviteUser_SendsInvite_WhenDataGood()
    {
        const string gameId = "this-game";
        const string jwt = "jwt-token";
        const string key = "this-key";
        const string feUri = "fe-uri";
        SetUpSecrets(feUri: feUri);
        var guild = new Mock<IDiscordGuild>();
        var user = new Mock<IDiscordGuildUser>();
        var dmChannel = new Mock<IDiscordDmChannel>();
        _mockGameStateStore.Setup(o => o.Get(gameId)).Returns(new GameState { GuildId = "1" });
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(guild.Object);
        guild.Setup(o => o.GetUser(UserId)).Returns(user.Object);
        user.Setup(o => o.Id).Returns(UserId);
        user.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(dmChannel.Object);
        _mockJwtWriter.Setup(o => o.GetJwtToken(It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player))).Returns(jwt);
        _mockIdGenerator.Setup(o => o.GenerateId()).Returns(key);
        var mockCacheEntry = new Mock<ICacheEntry>();
        _mockCache.Setup(o => o.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

        var (outcome, message) = await Sut.InviteUser(gameId, UserId);

        dmChannel.Verify(o => o.SendMessageAsync($"[Join here]({feUri + $"/join?key={key}"})"), Times.Once);
        _mockGameStateStore.Verify(o => o.AddUserToGame(gameId, It.Is<GameUser>(g => g.Id == UserId.ToString() && g.UserType == UserType.Player)), Times.Once);
        outcome.Should().Be(InviteUserOutcome.InviteSent);
        message.Should().Be("Sent message to user");
    }

    #endregion


    private static IEnumerable<object[]> GetGameTimeValues() => GetAllEnumValues<GameTime>();

    private static TownOccupants GetDummyTownOccupants()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var townOccupants = new TownOccupants(channelCategories);
        return townOccupants;
    }

    private static readonly MiniCategory DayCategory = new("day-category", "Day Category", [
        new ChannelOccupants(new MiniChannel("day-channel-1", "Day Channel 1"), [
            CommonMethods.GetRandomGameUser(),
        ]),
        new ChannelOccupants(new MiniChannel("day-channel-2", "Day Channel 2"), [
            CommonMethods.GetRandomGameUser(),
            CommonMethods.GetRandomGameUser()
        ])
    ]);

    private static readonly MiniCategory NightCategory = new("night-category", "Night Category", [
        new ChannelOccupants(new MiniChannel("night-channel-1", "Night Channel 1"), []),
        new ChannelOccupants(new MiniChannel("night-channel-2", "Night Channel 2"), [
            CommonMethods.GetRandomGameUser(),
        ]),
        new ChannelOccupants(new MiniChannel("night-channel-3", "Night Channel 3"), []),
    ]);
}