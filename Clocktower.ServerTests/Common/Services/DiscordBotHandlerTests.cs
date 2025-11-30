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
    private const ulong ChannelId1 = 1L;
    private const ulong ChannelId2 = 2L;
    private const ulong GuildId = 3L;
    private const ulong UserId = 4L;

    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<ITownOccupantManager> _mockTownOccupantManager = null!;
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IServiceScopeFactory> _mockServiceScopeFactory = null!;
    private Mock<IServiceScope> _mockScope = null!;
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<IDiscordTownService> _mockTownService = null!;

    private IDiscordBotHandler Sut => new DiscordBotHandler(
        _mockGameStateStore.Object,
        _mockTownOccupantManager.Object,
        _mockNotificationService.Object,
        _mockServiceScopeFactory.Object
    );

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

        _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IDiscordTownService))).Returns(_mockTownService.Object);
        _mockTownService.Setup(ts => ts.GetTownOccupancy(It.IsAny<ulong>()))
            .ReturnsAsync((false, null, "Test failure"));
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenNoGuildId()
    {
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();

        before.Setup(o => o.GuildId).Returns((ulong?)null);
        after.Setup(o => o.GuildId).Returns((ulong?)null);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenChannelsMatch()
    {
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var voiceChannel = new Mock<IDiscordVoiceChannel>();

        voiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns(GuildId);
        before.Setup(o => o.VoiceChannel).Returns(voiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns(voiceChannel.Object);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_CreatesTownService_WhenMoveOccurred()
    {
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var beforeVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var afterVoiceChannel = new Mock<IDiscordVoiceChannel>();

        beforeVoiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        afterVoiceChannel.Setup(o => o.Id).Returns(ChannelId2);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns(GuildId);
        before.Setup(o => o.VoiceChannel).Returns(beforeVoiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns(afterVoiceChannel.Object);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockServiceScopeFactory.Verify(f => f.CreateScope(), Times.Once);
        _mockServiceProvider.Verify(o => o.GetService(typeof(IDiscordTownService)), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_Exits_WhenTownOccupancyFails()
    {
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var beforeVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var afterVoiceChannel = new Mock<IDiscordVoiceChannel>();

        beforeVoiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        afterVoiceChannel.Setup(o => o.Id).Returns(ChannelId2);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns(GuildId);
        before.Setup(o => o.VoiceChannel).Returns(beforeVoiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns(afterVoiceChannel.Object);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockTownOccupantManager.Verify(o => o.MoveUser(
            It.IsAny<TownOccupants>(),
            It.IsAny<IDiscordUser>(),
            It.IsAny<IDiscordVoiceChannel>()
        ), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_MovesUser_WhenGotTownOccupancy()
    {
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var beforeVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var afterVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var dummyTownOccupancy = GetDummyTownOccupants();

        beforeVoiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        afterVoiceChannel.Setup(o => o.Id).Returns(ChannelId2);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns(GuildId);
        before.Setup(o => o.VoiceChannel).Returns(beforeVoiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns(afterVoiceChannel.Object);
        _mockTownService.Setup(o => o.GetTownOccupancy(GuildId)).ReturnsAsync((true, dummyTownOccupancy, string.Empty));

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockTownOccupantManager.Verify(o => o.MoveUser(
            dummyTownOccupancy,
            user.Object,
            afterVoiceChannel.Object
        ), Times.Once);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_DoesNotNotifyClients_WhenNoGameState()
    {
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var beforeVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var afterVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var dummyTownOccupancy = GetDummyTownOccupants();

        beforeVoiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        afterVoiceChannel.Setup(o => o.Id).Returns(ChannelId2);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns(GuildId);
        before.Setup(o => o.VoiceChannel).Returns(beforeVoiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns(afterVoiceChannel.Object);
        _mockTownService.Setup(o => o.GetTownOccupancy(GuildId)).ReturnsAsync((true, dummyTownOccupancy, string.Empty));
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([]);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(It.IsAny<string>(), It.IsAny<TownOccupants>()), Times.Never);
        _mockNotificationService.Verify(o => o.BroadcastUserVoiceStateChanged(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_NotifyClients_InChannel_WhenHasGameState()
    {
        const string gameId = "game-id";
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var beforeVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var afterVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var dummyTownOccupancy = GetDummyTownOccupants();
        var dummyTownOccupancy2 = new TownOccupants([]);

        user.Setup(o => o.Id).Returns(UserId);
        beforeVoiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        afterVoiceChannel.Setup(o => o.Id).Returns(ChannelId2);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns(GuildId);
        before.Setup(o => o.VoiceChannel).Returns(beforeVoiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns((afterVoiceChannel.Object));
        _mockTownService.Setup(o => o.GetTownOccupancy(GuildId)).ReturnsAsync((true, dummyTownOccupancy, string.Empty));
        _mockTownOccupantManager.Setup(o => o.MoveUser(dummyTownOccupancy, user.Object, afterVoiceChannel.Object)).Returns(dummyTownOccupancy2);
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([new GameState { Id = gameId }]);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(gameId, dummyTownOccupancy2), Times.Once);
        _mockNotificationService.Verify(o => o.BroadcastUserVoiceStateChanged(gameId, UserId.ToString(), true), Times.Once);
    }


    [TestMethod]
    public async Task HandleUserVoiceStateUpdate_NotifyClients_NotInChannel_WhenHasGameState()
    {
        const string gameId = "game-id";
        var user = new Mock<IDiscordUser>();
        var before = new Mock<IDiscordVoiceState>();
        var after = new Mock<IDiscordVoiceState>();
        var beforeVoiceChannel = new Mock<IDiscordVoiceChannel>();
        var dummyTownOccupancy = GetDummyTownOccupants();
        var dummyTownOccupancy2 = new TownOccupants([]);

        user.Setup(o => o.Id).Returns(UserId);
        beforeVoiceChannel.Setup(o => o.Id).Returns(ChannelId1);
        before.Setup(o => o.GuildId).Returns(GuildId);
        after.Setup(o => o.GuildId).Returns((ulong?)null);
        before.Setup(o => o.VoiceChannel).Returns(beforeVoiceChannel.Object);
        after.Setup(o => o.VoiceChannel).Returns((IDiscordVoiceChannel?)null);
        _mockTownService.Setup(o => o.GetTownOccupancy(GuildId)).ReturnsAsync((true, dummyTownOccupancy, string.Empty));
        _mockTownOccupantManager.Setup(o => o.MoveUser(dummyTownOccupancy, user.Object, null)).Returns(dummyTownOccupancy2);
        _mockGameStateStore.Setup(o => o.GetGuildGames(GuildId)).Returns([new GameState { Id = gameId }]);

        await Sut.HandleUserVoiceStateUpdate(user.Object, before.Object, after.Object);

        _mockNotificationService.Verify(o => o.BroadcastTownOccupancyUpdate(gameId, dummyTownOccupancy2), Times.Once);
        _mockNotificationService.Verify(o => o.BroadcastUserVoiceStateChanged(gameId, UserId.ToString(), false), Times.Once);
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
}