using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Town.Services;
using Clocktower.Server.Socket;
using Microsoft.Extensions.DependencyInjection;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class DiscordBotHandlerTests
{
    private IDiscordBotHandler Sut => _mockHandler.Object;

    private const string ChannelId1 = "1";
    private const string ChannelId2 = "2";
    private const string GuildId = "3";
    private const string UserId = "4";

    private Mock<DiscordBotHandler> _mockHandler = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
    private Mock<IServiceScope> _mockScope = null!;
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<IDiscordTownService> _mockTownService = null!;
    private Mock<IDiscordUser> _user = null!;
    private Mock<IDiscordGuildUser> _guildUser = null!;
    private Mock<IDiscordVoiceState> _before = null!;
    private Mock<IDiscordVoiceState> _after = null!;
    private Mock<IDiscordVoiceChannel> _voiceChannel1 = null!;
    private Mock<IDiscordVoiceChannel> _voiceChannel2 = null!;


    [TestInitialize]
    public void SetUp()
    {
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockDiscordTownManager = new Mock<IDiscordTownManager>();
        _mockUserService = new Mock<IUserService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockTownService = new Mock<IDiscordTownService>();

        _user = new Mock<IDiscordUser>();
        _guildUser = new Mock<IDiscordGuildUser>();
        _before = new Mock<IDiscordVoiceState>();
        _after = new Mock<IDiscordVoiceState>();
        _voiceChannel1 = new Mock<IDiscordVoiceChannel>();
        _voiceChannel2 = new Mock<IDiscordVoiceChannel>();

        _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IDiscordTownService))).Returns(_mockTownService.Object);
        _mockTownService.Setup(ts => ts.GetDiscordTown(It.IsAny<string>()))
            .ReturnsAsync((false, null, "Test failure"));

        _mockHandler = new Mock<DiscordBotHandler>
        (
            _mockGameStateStore.Object,
            _mockDiscordTownManager.Object,
            _mockUserService.Object,
            _mockNotificationService.Object,
            _mockServiceScopeFactory.Object)
        {
            CallBase = true
        };
    }

    private void Setup_UpdateDiscordTown(DiscordTown? getDiscordTownValue = null, DiscordTown? moveUserValue = null)
    {
        Setup_Mocks();

        _mockTownService.Setup(o => o.GetDiscordTown(GuildId)).ReturnsAsync((getDiscordTownValue is not null, getDiscordTownValue, string.Empty));
        if (moveUserValue != null && getDiscordTownValue != null)
        {
            _mockDiscordTownManager.Setup(o => o.MoveUser(getDiscordTownValue, _guildUser.Object, It.IsAny<IDiscordVoiceChannel>())).Returns(moveUserValue);
        }
    }

    private void Setup_UpdateVoiceStatus(bool isServerMuted, bool isSelfMuted, bool isServerDeafened, bool isSelfDeafened, bool inVoice, bool updateUserResult = true, DiscordTown? getDiscordTownResult = null)
    {
        Setup_Mocks();
        _after.Setup(o => o.IsMuted).Returns(isServerMuted);
        _after.Setup(o => o.IsSelfMuted).Returns(isSelfMuted);
        _after.Setup(o => o.IsDeafened).Returns(isServerDeafened);
        _after.Setup(o => o.IsSelfDeafened).Returns(isSelfDeafened);
        _after.Setup(o => o.VoiceChannel).Returns(inVoice ? _voiceChannel2.Object : null);
        _mockDiscordTownManager.Setup(o => o.UpdateUserStatus(GuildId, UserId, inVoice, It.Is<VoiceState>(ms =>
            ms.IsSelfMuted == isSelfMuted &&
            ms.IsSelfDeafened == isSelfDeafened &&
            ms.IsServerDeafened == isServerDeafened &&
            ms.IsServerMuted == isServerMuted
        ))).Returns(updateUserResult);
        _mockDiscordTownManager.Setup(o => o.GetDiscordTown(GuildId)).Returns(getDiscordTownResult);
    }


    private void Setup_Mocks(string? beforeGuildId = GuildId, string? afterGuildId = GuildId, bool hasGuildUser = true, string[]? gameIds = null, Mock<IDiscordVoiceChannel>? beforeChannel = null, Mock<IDiscordVoiceChannel>? afterChannel = null)
    {
        _before.Setup(o => o.GuildId).Returns(beforeGuildId);
        _after.Setup(o => o.GuildId).Returns(afterGuildId);
        _user.Setup(o => o.Id).Returns(UserId);
        _guildUser.Setup(o => o.Id).Returns(UserId);
        if (hasGuildUser)
        {
            _user.Setup(o => o.GetGuildUser()).Returns(_guildUser.Object);
        }

        var gameStates = gameIds is null ? [] : gameIds.Select(o => new GameState { Id = o });
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns(gameStates);

        _voiceChannel1.Setup(o => o.Id).Returns(ChannelId1);
        _voiceChannel2.Setup(o => o.Id).Returns(ChannelId2);
        _before.Setup(o => o.VoiceChannel).Returns(beforeChannel is not null ? beforeChannel.Object : _voiceChannel1.Object);
        _after.Setup(o => o.VoiceChannel).Returns(afterChannel is not null ? afterChannel.Object : _voiceChannel2.Object);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenNoGuildId()
    {
        Setup_Mocks(beforeGuildId: null, afterGuildId: null);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenGuildUserNotFound()
    {
        Setup_Mocks(hasGuildUser: false);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockGameStateStore.Verify(f => f.GetGuildGames(It.IsAny<string>()), Times.Never);
    }


    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsGetGuildGames()
    {
        Setup_Mocks();

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockGameStateStore.Verify(f => f.GetGuildGames(GuildId), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsUpdateDiscordTown_WhenChannelsDoNotMatch()
    {
        const string gameId1 = "game-id1";
        const string gameId2 = "game-id2";
        string[] gameIds = [gameId1, gameId2];
        Setup_Mocks(gameIds: gameIds, beforeChannel: _voiceChannel1, afterChannel: _voiceChannel2);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateDiscordTown(_guildUser.Object, _after.Object, gameId1, GuildId), Times.Once);
        _mockHandler.Verify(x => x.UpdateDiscordTown(_guildUser.Object, _after.Object, gameId2, GuildId), Times.Once);
        _mockHandler.Verify(x => x.UpdateVoiceStatus(_guildUser.Object, _after.Object, gameId1, GuildId), Times.Never);
        _mockHandler.Verify(x => x.UpdateVoiceStatus(_guildUser.Object, _after.Object, gameId2, GuildId), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsUpdateDiscordTown_WhenChannelsMatch_WithGameState()
    {
        const string gameId1 = "game-id1";
        const string gameId2 = "game-id2";
        string[] gameIds = [gameId1, gameId2];
        Setup_Mocks(gameIds: gameIds, beforeChannel: _voiceChannel1, afterChannel: _voiceChannel1);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateVoiceStatus(_guildUser.Object, _after.Object, gameId1, GuildId), Times.Once);
        _mockHandler.Verify(x => x.UpdateVoiceStatus(_guildUser.Object, _after.Object, gameId2, GuildId), Times.Once);
        _mockHandler.Verify(x => x.UpdateDiscordTown(_guildUser.Object, _after.Object, gameId1, GuildId), Times.Never);
        _mockHandler.Verify(x => x.UpdateDiscordTown(_guildUser.Object, _after.Object, gameId2, GuildId), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsUpdateDiscordTown_WhenChannelsMatch_WithNoGameState()
    {
        Setup_Mocks(gameIds: [], beforeChannel: _voiceChannel1, afterChannel: _voiceChannel1);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateVoiceStatus(_guildUser.Object, _after.Object, It.IsAny<string>(), GuildId), Times.Never);
        _mockHandler.Verify(x => x.UpdateDiscordTown(_guildUser.Object, _after.Object, It.IsAny<string>(), GuildId), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsUpdateDiscordTown()
    {
        const string gameId = "game-id";
        Setup_Mocks(gameIds: [gameId, "game-id2"]);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateDiscordTown(_guildUser.Object, _after.Object, gameId, GuildId), Times.Once);
    }


    [TestMethod]
    public async Task UpdateDiscordTown_CreatesTownService()
    {
        Setup_UpdateDiscordTown();

        await Sut.UpdateDiscordTown(_guildUser.Object, _after.Object, It.IsAny<string>(), GuildId);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Once);
        _mockServiceProvider.Verify(o => o.GetService(typeof(IDiscordTownService)), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenDiscordTownFails()
    {
        Setup_UpdateDiscordTown(getDiscordTownValue: null);

        await Sut.UpdateDiscordTown(_guildUser.Object, _after.Object, It.IsAny<string>(), GuildId);

        _mockDiscordTownManager.Verify(o => o.MoveUser(
            It.IsAny<DiscordTown>(),
            It.IsAny<IDiscordGuildUser>(),
            It.IsAny<IDiscordVoiceChannel>()
        ), Times.Never);
    }

    [TestMethod]
    public async Task UpdateDiscordTown_MovesUser_WhenGotDiscordTown()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        Setup_UpdateDiscordTown(getDiscordTownValue: dummyDiscordTown);

        await Sut.UpdateDiscordTown(_guildUser.Object, _after.Object, It.IsAny<string>(), GuildId);

        _mockDiscordTownManager.Verify(o => o.MoveUser(
            dummyDiscordTown,
            _guildUser.Object,
            _voiceChannel2.Object
        ), Times.Once);
    }

    [TestMethod]
    public async Task UpdateDiscordTown_NotifiesClients_WhenNoGame()
    {
        var dummyDiscordTown = GetDummyDiscordTown();
        Setup_UpdateDiscordTown(getDiscordTownValue: dummyDiscordTown);

        await Sut.UpdateDiscordTown(_guildUser.Object, _after.Object, string.Empty, GuildId);

        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(
            string.Empty,
            It.IsAny<DiscordTown>()
        ), Times.Once);
    }

    [TestMethod]
    public async Task UpdateDiscordTown_NotifyClients_WhenHasGame()
    {
        const string gameId = "game-id";
        var dummyDiscordTown = GetDummyDiscordTown();
        var dummyDiscordTown2 = new DiscordTown([]);
        Setup_UpdateDiscordTown(getDiscordTownValue: dummyDiscordTown, moveUserValue: dummyDiscordTown2);

        await Sut.UpdateDiscordTown(_guildUser.Object, _after.Object, gameId, GuildId);

        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(gameId, dummyDiscordTown2), Times.Once);
    }

    [TestMethod]
    [DynamicData(nameof(GetVoiceStateCombinations))]
    public async Task HandleUserVoiceStateUpdate_UpdatesUser_And_NotifyClients(bool inVoice, bool isServerMuted, bool isSelfMuted, bool isServerDeafened, bool isSelfDeafened)
    {
        const string gameId = "game-id";
        var discordTown = new DiscordTown([new MiniCategory(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [])]);
        Setup_UpdateVoiceStatus(isServerMuted, isSelfMuted, isServerDeafened, isSelfDeafened, inVoice, updateUserResult: true, getDiscordTownResult: discordTown);

        await Sut.UpdateVoiceStatus(_guildUser.Object, _after.Object, gameId, GuildId);

        _mockUserService.Verify(o => o.UpdateDiscordPresence(UserId, GuildId,
            inVoice,
            It.Is<VoiceState>(voiceState =>
                voiceState.IsServerMuted == isServerMuted &&
                voiceState.IsSelfMuted == isSelfMuted &&
                voiceState.IsSelfDeafened == isSelfDeafened &&
                voiceState.IsServerDeafened == isServerDeafened
            )));
        _mockNotificationService.Verify(o => o.BroadcastDiscordTownUpdate(gameId, discordTown), Times.Once);
    }

    private static DiscordTown GetDummyDiscordTown()
    {
        var channelCategories = new List<MiniCategory> { DayCategory, NightCategory };
        var discordTown = new DiscordTown(channelCategories);
        return discordTown;
    }

    private static readonly MiniCategory DayCategory = new("day-category", "Day Category", [
        new ChannelOccupants(new MiniChannel("day-channel-1", "Day Channel 1"), [
            CommonMethods.GetRandomTownUser(),
        ]),
        new ChannelOccupants(new MiniChannel("day-channel-2", "Day Channel 2"), [
            CommonMethods.GetRandomTownUser(),
            CommonMethods.GetRandomTownUser()
        ])
    ]);

    private static readonly MiniCategory NightCategory = new("night-category", "Night Category", [
        new ChannelOccupants(new MiniChannel("night-channel-1", "Night Channel 1"), []),
        new ChannelOccupants(new MiniChannel("night-channel-2", "Night Channel 2"), [
            CommonMethods.GetRandomTownUser(),
        ]),
        new ChannelOccupants(new MiniChannel("night-channel-3", "Night Channel 3"), []),
    ]);

    public static IEnumerable<object[]> GetVoiceStateCombinations() => TestDataProvider.GenerateBooleanCombinations(5);
}