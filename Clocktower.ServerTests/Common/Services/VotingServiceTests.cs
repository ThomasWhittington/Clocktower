using Clocktower.Server.Common.Services;
using Clocktower.Server.Common.UpdateModels;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Socket.Services;

namespace Clocktower.ServerTests.Common.Services;

[TestClass]
public class VotingServiceTests
{
    private const string GameId = "game-id";
    private const string UserId = "user-id";
    private const string GuildId = "guild-id";
    private Mock<IGamePerspectiveService> _mockGamePerspectiveService = null!;
    private Mock<IGameBroadcastService> _mockGameBroadcastService = null!;
    private IVotingService _sut = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockGamePerspectiveService = new Mock<IGamePerspectiveService>();
        _mockGameBroadcastService = new Mock<IGameBroadcastService>();
        _sut = new VotingService(_mockGamePerspectiveService.Object, _mockGameBroadcastService.Object);
    }

    #region RemoveAllMarks

    [TestMethod]
    public async Task RemoveAllMarks_EarlyReturn_WhenNoGameFound()
    {
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns((GamePerspective?)null);

        await _sut.RemoveAllMarks(GameId);

        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, It.IsAny<string>(), It.IsAny<PublicGameUserUpdate>()), Times.Never);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task RemoveAllMarks_EarlyReturn_WhenNoMarkedUsers()
    {
        var gamePerspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow)
        {
            Users =
            [
                CommonMethods.GetRandomGameUser(),
                CommonMethods.GetRandomGameUser(),
                CommonMethods.GetRandomGameUser()
            ]
        };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(gamePerspective);

        await _sut.RemoveAllMarks(GameId);

        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, It.IsAny<string>(), It.IsAny<PublicGameUserUpdate>()), Times.Never);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    [TestMethod]
    public async Task RemoveAllMarks_UpdatesUsers()
    {
        var user1 = CommonMethods.GetRandomGameUser("user-1") with { IsMarked = true };
        var user2 = CommonMethods.GetRandomGameUser("user-2") with { IsMarked = false };
        var user3 = CommonMethods.GetRandomGameUser("user-3") with { IsMarked = true };
        var gamePerspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow)
        {
            Users = [user1, user2, user3]
        };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(gamePerspective);
        _mockGamePerspectiveService.Setup(o => o.UpdatePublicUser(GameId, It.IsAny<string>(), It.IsAny<PublicGameUserUpdate>())).Returns(true);


        await _sut.RemoveAllMarks(GameId);

        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, "user-1", new PublicGameUserUpdate { IsMarked = false }), Times.Once);
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, "user-3", new PublicGameUserUpdate { IsMarked = false }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Once);
    }

    [TestMethod]
    public async Task RemoveAllMarks_DoesNotCallBroadcast_WhenNoUpdate()
    {
        var user1 = CommonMethods.GetRandomGameUser("user-1") with { IsMarked = true };
        var user2 = CommonMethods.GetRandomGameUser("user-2") with { IsMarked = false };
        var user3 = CommonMethods.GetRandomGameUser("user-3") with { IsMarked = true };
        var gamePerspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow)
        {
            Users = [user1, user2, user3]
        };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(gamePerspective);
        _mockGamePerspectiveService.Setup(o => o.UpdatePublicUser(GameId, It.IsAny<string>(), It.IsAny<PublicGameUserUpdate>())).Returns(false);


        await _sut.RemoveAllMarks(GameId);

        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, "user-1", new PublicGameUserUpdate { IsMarked = false }), Times.Once);
        _mockGamePerspectiveService.Verify(o => o.UpdatePublicUser(GameId, "user-3", new PublicGameUserUpdate { IsMarked = false }), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastDiscordTownUpdate(GameId), Times.Never);
    }

    #endregion

    #region StartVote

    [TestMethod]
    public async Task StartVote_DoesNothing_WhenSessionNotFound()
    {
        await _sut.StartVote(GameId, 1000);

        _mockGameBroadcastService.Verify(o => o.BroadcastNominationSessionUpdate(It.IsAny<string>(), It.IsAny<NominationSession>()), Times.Never);
    }

    [TestMethod]
    public async Task StartVote_DoesNothing_WhenNominatorIsNull()
    {
        await _sut.OpenNominations(GameId);
        var existingSession = _sut.GetSession(GameId);
        if (existingSession != null)
        {
            typeof(NominationSession).GetProperty(nameof(NominationSession.Nominator))?.SetValue(existingSession, null);
            typeof(NominationSession).GetProperty(nameof(NominationSession.Nominee))?.SetValue(existingSession, 1);
        }

        _mockGameBroadcastService.Invocations.Clear();

        await _sut.StartVote(GameId, 1000);

        _mockGameBroadcastService.Verify(o => o.BroadcastNominationSessionUpdate(GameId, It.Is<NominationSession>(s => s.VoteUnderway)), Times.Never);
    }

    [TestMethod]
    public async Task StartVote_DoesNothing_WhenNomineeIsNull()
    {
        await _sut.OpenNominations(GameId);
        var existingSession = _sut.GetSession(GameId);
        if (existingSession != null)
        {
            typeof(NominationSession).GetProperty(nameof(NominationSession.Nominator))?.SetValue(existingSession, 0);
            typeof(NominationSession).GetProperty(nameof(NominationSession.Nominee))?.SetValue(existingSession, null);
        }

        _mockGameBroadcastService.Invocations.Clear();

        await _sut.StartVote(GameId, 1000);

        _mockGameBroadcastService.Verify(o => o.BroadcastNominationSessionUpdate(GameId, It.Is<NominationSession>(s => s.VoteUnderway)), Times.Never);
    }

    [TestMethod]
    public async Task StartVote_StartsVoteWithCorrectSettings()
    {
        var gamePerspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow)
        {
            Users =
            [
                CommonMethods.GetRandomGameUser() with { UserType = UserType.Player, SeatingPosition = 0 },
                CommonMethods.GetRandomGameUser() with { UserType = UserType.Player, SeatingPosition = 1 },
                CommonMethods.GetRandomGameUser() with { UserType = UserType.Player, SeatingPosition = 2 }
            ]
        };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(gamePerspective);

        await _sut.OpenNominations(GameId);
        await _sut.MakeNomination(GameId, gamePerspective.Users[0].Id, gamePerspective.Users[2].Id);

        _mockGameBroadcastService.Invocations.Clear();

        const int votingSpeed = 1500;
        await _sut.StartVote(GameId, votingSpeed);

        _mockGameBroadcastService.Verify(o => o.BroadcastNominationSessionUpdate(GameId, It.Is<NominationSession>(s =>
            s.VoteUnderway &&
            s.VotingSpeed == votingSpeed &&
            s.CountDown == 3 &&
            s.NextTick > DateTime.UtcNow
        )), Times.Once);
    }

    [TestMethod]
    public async Task StartVote_UpdatesSession()
    {
        var gamePerspective = new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow)
        {
            Users =
            [
                CommonMethods.GetRandomGameUser() with { UserType = UserType.Player, SeatingPosition = 0 },
                CommonMethods.GetRandomGameUser() with { UserType = UserType.Player, SeatingPosition = 1 }
            ]
        };
        _mockGamePerspectiveService.Setup(o => o.GetFirstPerspective(GameId)).Returns(gamePerspective);

        await _sut.OpenNominations(GameId);
        await _sut.MakeNomination(GameId, gamePerspective.Users[0].Id, gamePerspective.Users[1].Id);

        const int votingSpeed = 2000;
        await _sut.StartVote(GameId, votingSpeed);

        var session = _sut.GetSession(GameId);
        session.Should().NotBeNull();
        session!.VoteUnderway.Should().BeTrue();
        session.VotingSpeed.Should().Be(votingSpeed);
        session.CountDown.Should().Be(3);
        session.NextTick.Should().BeAfter(DateTime.UtcNow);
    }

    #endregion

    #region MakeNomination

    [TestMethod]
    public async Task MakeNomination_EarlyReturns_WhenNoSession()
    {
        await _sut.MakeNomination(GameId, "0", "0");

        _mockGamePerspectiveService.Verify(o => o.GetFirstPerspective(GameId), Times.Never());
    }

    [TestMethod]
    public async Task MakeNomination_EarlyReturns_WhenNoGame()
    {
        _mockGamePerspectiveService.SetupSequence(o => o.GetFirstPerspective(GameId))
            .Returns(new GamePerspective(GameId, UserId, GuildId, CommonMethods.GetRandomGameUser(), DateTime.UtcNow))
            .Returns((GamePerspective?)null);
        await _sut.OpenNominations(GameId);

        _mockGamePerspectiveService.Invocations.Clear();

        await _sut.MakeNomination(GameId, "0", "0");

        _mockGamePerspectiveService.Verify(o => o.GetFirstPerspective(GameId), Times.Once);
        _mockGameBroadcastService.Verify(o => o.BroadcastNominationSessionUpdate(GameId, It.Is<NominationSession>(s => s.VoteUnderway)), Times.Never);
    }

    #endregion
}