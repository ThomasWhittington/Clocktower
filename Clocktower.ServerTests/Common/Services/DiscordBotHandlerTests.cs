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
   
    private const ulong ChannelId1 = 1L;
    private const ulong ChannelId2 = 2L;
    private const ulong GuildId = 3L;
    private const ulong UserId = 4L;

    private Mock<DiscordBotHandler> _mockHandler = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<ITownOccupantManager> _mockTownOccupantManager = null!;
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
        _mockTownOccupantManager = new Mock<ITownOccupantManager>();
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
        _mockTownService.Setup(ts => ts.GetTownOccupancy(It.IsAny<ulong>()))
            .ReturnsAsync((false, null, "Test failure"));

        _mockHandler = new Mock<DiscordBotHandler>
        (
            _mockGameStateStore.Object,
            _mockTownOccupantManager.Object,
            _mockNotificationService.Object,
            _mockServiceScopeFactory.Object)
        {
            CallBase = true
        };
    }

    private void Setup_UpdateTownOccupancy(Mock<IDiscordVoiceChannel>? beforeChannel = null, Mock<IDiscordVoiceChannel>? afterChannel = null, TownOccupants? getTownOccupancyValue = null, TownOccupants? moveUserValue = null)
    {
        Setup();
        _voiceChannel1.Setup(o => o.Id).Returns(ChannelId1);
        _voiceChannel2.Setup(o => o.Id).Returns(ChannelId2);
        _before.Setup(o => o.VoiceChannel).Returns(beforeChannel is not null ? beforeChannel.Object : _voiceChannel1.Object);
        _after.Setup(o => o.VoiceChannel).Returns(afterChannel is not null ? afterChannel.Object : _voiceChannel2.Object);
        _mockTownService.Setup(o => o.GetTownOccupancy(GuildId)).ReturnsAsync((getTownOccupancyValue is not null, getTownOccupancyValue, string.Empty));
        if (moveUserValue != null && getTownOccupancyValue != null)
        {
            _mockTownOccupantManager.Setup(o => o.MoveUser(getTownOccupancyValue, _guildUser.Object, It.IsAny<IDiscordVoiceChannel>())).Returns(moveUserValue);
        }
    }

    private void Setup_UpdateMutedStatus(bool isServerMuted, bool isSelfMuted, bool isServerDeafened, bool isSelfDeafened, bool inVoice)
    {
        Setup();
        _after.Setup(o => o.IsMuted).Returns(isServerMuted);
        _after.Setup(o => o.IsSelfMuted).Returns(isSelfMuted);
        _after.Setup(o => o.IsDeafened).Returns(isServerDeafened);
        _after.Setup(o => o.IsSelfDeafened).Returns(isSelfDeafened);
        _after.Setup(o => o.VoiceChannel).Returns(inVoice ? _voiceChannel2.Object : null);
    }


    private void Setup(ulong? beforeGuildId = GuildId, ulong? afterGuildId = GuildId, bool hasGuildUser = true, string[]? gameIds = null)
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
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenNoGuildId()
    {
        Setup(beforeGuildId: null, afterGuildId: null);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenGuildUserNotFound()
    {
        Setup(hasGuildUser: false);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockGameStateStore.Verify(f => f.GetGuildGames(It.IsAny<string>()), Times.Never);
    }


    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsGetGuildGames()
    {
        Setup();

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockGameStateStore.Verify(f => f.GetGuildGames(GuildId), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsUpdateTownOccupancy()
    {
        const string gameId = "game-id";
        Setup(gameIds: [gameId, "game-id2"]);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, gameId, GuildId), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CallsUpdateMutedStatus_WhenHasGameState()
    {
        string[] gameIds = ["game-id1", "game-id2"];
        Setup(gameIds: gameIds);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateMutedStatus(_guildUser.Object, _before.Object, _after.Object, gameIds[0]), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_NotCallUpdateMutedStatus_WhenHasNoGameState()
    {
        Setup(gameIds: []);

        await Sut.HandleUserVoiceStateUpdate(_user.Object, _before.Object, _after.Object);

        _mockHandler.Verify(x => x.UpdateMutedStatus(_guildUser.Object, _before.Object, _after.Object, It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateTownOccupancy_Exits_WhenChannelsMatch()
    {
        Setup_UpdateTownOccupancy(beforeChannel: _voiceChannel1, afterChannel: _voiceChannel1);

        await Sut.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, null, GuildId);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [TestMethod]
    public async Task UpdateTownOccupancy_CreatesTownService_WhenMoveOccurred()
    {
        Setup_UpdateTownOccupancy();

        await Sut.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, null, GuildId);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Once);
        _mockServiceProvider.Verify(o => o.GetService(typeof(IDiscordTownService)), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenTownOccupancyFails()
    {
        Setup_UpdateTownOccupancy(getTownOccupancyValue: null);

        await Sut.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, null, GuildId);

        _mockTownOccupantManager.Verify(o => o.MoveUser(
            It.IsAny<TownOccupants>(),
            It.IsAny<IDiscordGuildUser>(),
            It.IsAny<IDiscordVoiceChannel>()
        ), Times.Never);
    }

    [TestMethod]
    public async Task UpdateTownOccupancy_MovesUser_WhenGotTownOccupancy()
    {
        var dummyTownOccupancy = GetDummyTownOccupants();
        Setup_UpdateTownOccupancy(getTownOccupancyValue: dummyTownOccupancy);

        await Sut.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, null, GuildId);

        _mockTownOccupantManager.Verify(o => o.MoveUser(
            dummyTownOccupancy,
            _guildUser.Object,
            _voiceChannel2.Object
        ), Times.Once);
    }

    [TestMethod]
    public async Task UpdateTownOccupancy_DoesNotNotifyClients_WhenNoGame()
    {
        var dummyTownOccupancy = GetDummyTownOccupants();
        Setup_UpdateTownOccupancy(getTownOccupancyValue: dummyTownOccupancy);

        await Sut.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, null, GuildId);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(
            It.IsAny<string>(),
            It.IsAny<TownOccupants>()
        ), Times.Never);
    }

    [TestMethod]
    public async Task UpdateTownOccupancy_NotifyClients_WhenHasGame()
    {
        const string gameId = "game-id";
        var dummyTownOccupancy = GetDummyTownOccupants();
        var dummyTownOccupancy2 = new TownOccupants([]);
        Setup_UpdateTownOccupancy(getTownOccupancyValue: dummyTownOccupancy, moveUserValue: dummyTownOccupancy2);

        await Sut.UpdateTownOccupancy(_guildUser.Object, _before.Object, _after.Object, gameId, GuildId);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(gameId, dummyTownOccupancy2), Times.Once);
    }

    [TestMethod]
    [DynamicData(nameof(GetMutedStateCombinations))]
    public async Task HandleUserVoiceStateUpdate_UpdatesUser_And_NotifyClients(bool inVoice, bool isServerMuted, bool isSelfMuted, bool isServerDeafened, bool isSelfDeafened)
    {
        const string gameId = "game-id";
        Setup_UpdateMutedStatus(isServerMuted, isSelfMuted, isServerDeafened, isSelfDeafened, inVoice);

        await Sut.UpdateMutedStatus(_guildUser.Object, _before.Object, _after.Object, gameId);

        _mockGameStateStore.Verify(o => o.UpdateUser(gameId, UserId, null, null,
            isPresent: inVoice,
            discordMutedState: It.Is<MutedState>(ms =>
                ms.IsSelfMuted == isSelfMuted &&
                ms.IsSelfDeafened == isSelfDeafened &&
                ms.IsServerDeafened == isServerDeafened &&
                ms.IsServerMuted == isServerMuted
            )
        ), Times.Once);
        _mockNotificationService.Verify(o => o.BroadcastUserVoiceStateChanged(gameId, UserId.ToString(), inVoice,
            It.Is<MutedState>(ms =>
                ms.IsSelfMuted == isSelfMuted &&
                ms.IsSelfDeafened == isSelfDeafened &&
                ms.IsServerDeafened == isServerDeafened &&
                ms.IsServerMuted == isServerMuted
            )
        ), Times.Once);
    }

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

    public static IEnumerable<object[]> GetMutedStateCombinations() => TestDataProvider.GenerateBooleanCombinations(5);
}