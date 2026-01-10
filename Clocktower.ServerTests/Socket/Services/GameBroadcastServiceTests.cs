using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Dto;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket;
using Clocktower.Server.Socket.Services;

namespace Clocktower.ServerTests.Socket.Services;

[TestClass]
public class GameBroadcastServiceTests
{
    private const string GameId = "game-id";
    private const string GuildId = "guild-id";
    private const string UserId = "user-id";

    private Mock<IGamePerspectiveStore> _mockGamePerspectiveStore = null!;
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<INotificationService> _mockNotificationService = null!;

    private IGameBroadcastService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockGamePerspectiveStore = new Mock<IGamePerspectiveStore>();
        _mockDiscordTownManager = new Mock<IDiscordTownManager>();
        _mockNotificationService = StrictMockFactory.Create<INotificationService>();

        _sut = new GameBroadcastService(
            _mockGamePerspectiveStore.Object,
            _mockDiscordTownManager.Object,
            _mockNotificationService.Object
        );
    }


    #region BroadcastDiscordTownUpdate

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_NoPerspectives_DoesNothing()
    {
        _mockGamePerspectiveStore.Setup(x => x.GetAllPerspectivesForGame(GameId)).Returns([]);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        _mockNotificationService.Verify(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()), Times.Never);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_SinglePlayer_SendsRedactedTown()
    {
        var user = CommonMethods.GetRandomGameUser(UserId)with { UserType = UserType.Player };

        var perspective = CommonMethods.GetGamePerspective(GameId, UserId, GuildId);
        perspective = perspective with { Users = new List<GameUser> { user } };

        var town = new DiscordTownDto(GameId, new List<MiniCategoryDto>());
        var redactedTown = new DiscordTownDto(GameId, new List<MiniCategoryDto>());

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { perspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, perspective.Users))
            .Returns(town);

        _mockDiscordTownManager
            .Setup(x => x.RedactTownDto(town, UserId))
            .Returns(redactedTown);

        List<UserNotification>? capturedNotifications = null;
        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Callback<IEnumerable<UserNotification>>(notifications => capturedNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(1);
        capturedNotifications[0].UserId.Should().Be(UserId);
        capturedNotifications[0].Town.Should().Be(redactedTown);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_MultiplePlayers_SendsRedactedTownToEach()
    {
        var userId1 = CommonMethods.GetRandomSnowflakeStringId();
        var userId2 = CommonMethods.GetRandomSnowflakeStringId();

        var user1 = CommonMethods.GetRandomGameUser(userId1)with { UserType = UserType.Player };
        var user2 = CommonMethods.GetRandomGameUser(userId2)with { UserType = UserType.Player };

        var users1 = new List<GameUser> { user1, user2 };
        var users2 = new List<GameUser> { user1, user2 };

        var perspective1 = CommonMethods.GetGamePerspective(GameId, userId1, GuildId);
        perspective1 = perspective1 with { Users = users1 };

        var perspective2 = CommonMethods.GetGamePerspective(GameId, userId2, GuildId);
        perspective2 = perspective2 with { Users = users2 };

        var town1 = new DiscordTownDto(GameId, []);
        var town2 = new DiscordTownDto(GameId, []);
        var redactedTown1 = new DiscordTownDto(GameId, []);
        var redactedTown2 = new DiscordTownDto(GameId, []);

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { perspective1, perspective2 });

        _mockDiscordTownManager
            .SetupSequence(x => x.GetDiscordTownDto(GuildId, GameId, It.IsAny<IReadOnlyList<GameUser>>()))
            .Returns(town1)
            .Returns(town2);

        _mockDiscordTownManager
            .Setup(x => x.RedactTownDto(town1, userId1))
            .Returns(redactedTown1);

        _mockDiscordTownManager
            .Setup(x => x.RedactTownDto(town2, userId2))
            .Returns(redactedTown2);

        List<UserNotification>? capturedNotifications = null;
        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Callback<IEnumerable<UserNotification>>(notifications => capturedNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(2);
        capturedNotifications.Any(n => n.UserId == userId1 && n.Town == redactedTown1).Should().BeTrue();
        capturedNotifications.Any(n => n.UserId == userId2 && n.Town == redactedTown2).Should().BeTrue();
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_OmniscientPerspective_SendsUnredactedTownToStoryTeller()
    {
        const string storyTellerId = "storyteller-id";
        var storyTeller = CommonMethods.GetRandomGameUser(storyTellerId)with { UserType = UserType.StoryTeller };

        var omniscientPerspective = CommonMethods.GetGamePerspective(GameId, IGamePerspectiveStore.OmniscientKey, GuildId);
        omniscientPerspective = omniscientPerspective with { Users = new List<GameUser> { storyTeller } };

        var omniscientTown = new DiscordTownDto(GameId, new List<MiniCategoryDto>());

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { omniscientPerspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, omniscientPerspective.Users))
            .Returns(omniscientTown);

        List<UserNotification>? capturedNotifications = null;
        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Callback<IEnumerable<UserNotification>>(notifications => capturedNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(1);
        capturedNotifications[0].UserId.Should().Be(storyTellerId);
        capturedNotifications[0].Town.Should().Be(omniscientTown);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_OmniscientPerspective_SendsUnredactedTownToSpectator()
    {
        const string spectatorId = "spectator-id";
        var spectator = CommonMethods.GetRandomGameUser(spectatorId)with { UserType = UserType.Spectator };

        var omniscientPerspective = CommonMethods.GetGamePerspective(GameId, IGamePerspectiveStore.OmniscientKey, GuildId);
        omniscientPerspective = omniscientPerspective with { Users = new List<GameUser> { spectator } };

        var omniscientTown = new DiscordTownDto(GameId, new List<MiniCategoryDto>());

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { omniscientPerspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, omniscientPerspective.Users))
            .Returns(omniscientTown);

        List<UserNotification>? capturedNotifications = null;
        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Callback<IEnumerable<UserNotification>>(notifications => capturedNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(1);
        capturedNotifications[0].UserId.Should().Be(spectatorId);
        capturedNotifications[0].Town.Should().Be(omniscientTown);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_OmniscientPerspectiveWithMultipleOmniscientUsers_SendsToAll()
    {
        const string storyTellerId = "storyteller-id";
        const string spectatorId = "spectator-id";
        const string playerId = "player-id";

        var storyTeller = CommonMethods.GetRandomGameUser(storyTellerId) with { UserType = UserType.StoryTeller };
        var spectator = CommonMethods.GetRandomGameUser(spectatorId)with { UserType = UserType.Spectator };
        var player = CommonMethods.GetRandomGameUser(playerId)with { UserType = UserType.Player };

        var omniscientPerspective = CommonMethods.GetGamePerspective(GameId, IGamePerspectiveStore.OmniscientKey, GuildId);
        omniscientPerspective = omniscientPerspective with { Users = new List<GameUser> { storyTeller, spectator, player } };

        var omniscientTown = new DiscordTownDto(GameId, []);

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { omniscientPerspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, omniscientPerspective.Users))
            .Returns(omniscientTown);

        List<UserNotification>? capturedNotifications = null;
        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Callback<IEnumerable<UserNotification>>(notifications => capturedNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(2);
        capturedNotifications.Any(n => n.UserId == spectatorId && n.Town == omniscientTown).Should().BeTrue();
        capturedNotifications.Any(n => n.UserId == storyTellerId && n.Town == omniscientTown).Should().BeTrue();
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_PlayersAndOmniscient_SendsBothRedactedAndUnredacted()
    {
        const string playerId = "player-id";
        const string storyTellerId = "storyteller-id";
        var storyTeller = CommonMethods.GetRandomGameUser(storyTellerId) with { UserType = UserType.StoryTeller };
        var player = CommonMethods.GetRandomGameUser(playerId) with { UserType = UserType.Player };

        var playerUsers = new List<GameUser> { player, storyTeller };
        var omniscientUsers = new List<GameUser> { player, storyTeller };

        var playerPerspective = CommonMethods.GetGamePerspective(GameId, playerId, GuildId) with { Users = playerUsers };

        var omniscientPerspective = CommonMethods.GetGamePerspective(GameId, IGamePerspectiveStore.OmniscientKey, GuildId) with { Users = omniscientUsers };

        var playerTown = new DiscordTownDto(GameId, []);
        var redactedPlayerTown = new DiscordTownDto(GameId, []);
        var omniscientTown = new DiscordTownDto(GameId, []);

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { playerPerspective, omniscientPerspective });

        _mockDiscordTownManager
            .SetupSequence(x => x.GetDiscordTownDto(GuildId, GameId, It.IsAny<IReadOnlyList<GameUser>>()))
            .Returns(playerTown)
            .Returns(omniscientTown);

        _mockDiscordTownManager
            .Setup(x => x.RedactTownDto(playerTown, playerId))
            .Returns(redactedPlayerTown);

        List<UserNotification>? capturedNotifications = null;
        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Callback<IEnumerable<UserNotification>>(notifications => capturedNotifications = notifications.ToList())
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        capturedNotifications.Should().NotBeNull();
        capturedNotifications.Should().HaveCount(2);
        capturedNotifications.Any(n => n.UserId == playerId && n.Town == redactedPlayerTown).Should().BeTrue();
        capturedNotifications.Any(n => n.UserId == storyTellerId && n.Town == omniscientTown).Should().BeTrue();
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_PlayerTownIsNull_SkipsPlayer()
    {
        var user = CommonMethods.GetRandomGameUser(UserId) with { UserType = UserType.Player };
        var perspective = CommonMethods.GetGamePerspective(GameId, UserId, GuildId)with { Users = new List<GameUser> { user } };

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { perspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, perspective.Users))
            .Returns((DiscordTownDto?)null);

        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        _mockNotificationService.Verify(
            x => x.SendBulkDiscordTownUpdates(
                It.Is<IEnumerable<UserNotification>>(notifications => !notifications.Any())),
            Times.Once);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_OmniscientTownIsNull_SkipsOmniscientUsers()
    {
        const string storyTellerId = "storyteller-id";
        var storyTeller = CommonMethods.GetRandomGameUser(storyTellerId)with { UserType = UserType.StoryTeller };

        var omniscientPerspective = CommonMethods.GetGamePerspective(GameId, IGamePerspectiveStore.OmniscientKey, GuildId);
        omniscientPerspective = omniscientPerspective with { Users = new List<GameUser> { storyTeller } };

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { omniscientPerspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, omniscientPerspective.Users))
            .Returns((DiscordTownDto?)null);

        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        _mockNotificationService.Verify(
            x => x.SendBulkDiscordTownUpdates(
                It.Is<IEnumerable<UserNotification>>(notifications => !notifications.Any())),
            Times.Once);
    }

    [TestMethod]
    public async Task BroadcastDiscordTownUpdate_SkipsOmniscientKeyInRegularProcessing()
    {
        var omniscientPerspective = CommonMethods.GetGamePerspective(GameId, IGamePerspectiveStore.OmniscientKey, GuildId);
        omniscientPerspective = omniscientPerspective with { Users = new List<GameUser>() };

        _mockGamePerspectiveStore
            .Setup(x => x.GetAllPerspectivesForGame(GameId))
            .Returns(new List<GamePerspective> { omniscientPerspective });

        _mockDiscordTownManager
            .Setup(x => x.GetDiscordTownDto(GuildId, GameId, omniscientPerspective.Users))
            .Returns(new DiscordTownDto(GameId, []));

        _mockNotificationService
            .Setup(x => x.SendBulkDiscordTownUpdates(It.IsAny<IEnumerable<UserNotification>>()))
            .Returns(Task.CompletedTask);

        await _sut.BroadcastDiscordTownUpdate(GameId);

        _mockDiscordTownManager.Verify(
            x => x.RedactTownDto(It.IsAny<DiscordTownDto>(), IGamePerspectiveStore.OmniscientKey),
            Times.Never);
    }

    #endregion

    [TestMethod]
    public async Task BroadcastTimerUpdate_CallsNotificationService()
    {
        var timer = new TimerState
        {
            GameId = GameId,
            Status = TimerStatus.Running,
            ServerNowUtc = DateTime.UtcNow,
            EndUtc = DateTime.UtcNow.AddSeconds(30),
            Label = "label"
        };
        _mockNotificationService.Setup(c => c.SendTimerUpdateToGroup(GameId, timer)).Returns(Task.CompletedTask);

        await _sut.BroadcastTimerUpdate(GameId, timer);

        _mockNotificationService.Verify(c => c.SendTimerUpdateToGroup(GameId, timer), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastUserVoiceStateChanged_CallsNotificationService()
    {
        const bool inVoice = true;
        var voiceState = new VoiceState(true, false, true, false);
        _mockNotificationService.Setup(o => o.SendUserVoiceStateToGroup(GameId, UserId, inVoice, voiceState)).Returns(Task.CompletedTask);

        await _sut.BroadcastUserVoiceStateChanged(GameId, UserId, inVoice, voiceState);

        _mockNotificationService.Verify(c => c.SendUserVoiceStateToGroup(GameId, UserId, inVoice, voiceState), Times.Once);
    }

    [TestMethod]
    public async Task PingUser_CallsNotificationService()
    {
        const string message = "Test ping message";
        _mockNotificationService.Setup(c => c.PingUser(UserId, message)).Returns(Task.CompletedTask);

        await _sut.PingUser(UserId, message);

        _mockNotificationService.Verify(c => c.PingUser(UserId, message), Times.Once);
    }

    [TestMethod]
    public async Task BroadcastTimeUpdate_CallsNotificationService()
    {
        const GameTime gameTime = GameTime.Day;
        _mockNotificationService.Setup(c => c.SendTownTimeToGroup(GameId, gameTime)).Returns(Task.CompletedTask);

        await _sut.BroadcastTimeUpdate(GameId, gameTime);

        _mockNotificationService.Verify(c => c.SendTownTimeToGroup(GameId, gameTime), Times.Once);
    }
}