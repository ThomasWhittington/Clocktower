using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.GameAction.Services;

namespace Clocktower.ServerTests.Discord.GameAction.Services;

[TestClass]
public class DiscordGameActionServiceTests
{
    private const string GameId = "game-id";
    private const string GuildId = "123";
    private IDiscordGameActionService _sut = null!;
    private Mock<IDiscordBot> _mockBot = null!;
    private Mock<IGameStateStore> _mockGameStateStore = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<IDiscordGuild> _guild = null!;
    private GameState _gameState = null!;
    Func<TownUser, bool> capturedPredicate = null!;


    private void SetUp_Bot(bool hasGuild)
    {
        _mockBot.Setup(o => o.GetGuild(ulong.Parse(GuildId))).Returns(hasGuild ? _guild.Object : null);
    }

    private void SetUp_GameStateStore(bool hasGame, bool checkMuted = false, string guildId = GuildId, (string userId, bool isPresent, bool muted, UserType userType)[]? users = null)
    {
        _gameState.Id = GameId;
        _gameState.GuildId = guildId;
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(hasGame ? _gameState : null);

        if (users is not null)
        {
            var gameUsers = users.Select(user => new GameUser(user.userId) { UserType = user.userType }).ToList();
            var townUsers = users.Where(o => o.muted == !checkMuted && o is { isPresent: true, userType: UserType.StoryTeller }).Select(user =>
            {
                var thisUser = CommonMethods.GetRandomTownUser(user.userId);
                thisUser.IsPresent = true;
                thisUser.VoiceState = new VoiceState(user.muted, false, false, false);
                return thisUser;
            }).ToList();

            _gameState.Users.AddRange(gameUsers);
            _mockUserService.Setup(o => o.GetTownUsersForGameUsers(_gameState.StoryTellers, guildId, It.IsAny<Func<TownUser, bool>>()))
                .Callback<IEnumerable<GameUser>, string, Func<TownUser, bool>>((_, _, predicate) => { capturedPredicate = predicate; })
                .Returns(townUsers);
        }
    }

    [TestInitialize]
    public void SetUp()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockUserService = new Mock<IUserService>();
        _gameState = new GameState();
        _guild = new Mock<IDiscordGuild>();

        _sut = new DiscordGameActionService(_mockBot.Object, _mockGameStateStore.Object, _mockUserService.Object);
    }

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_ReturnsError_WhenNoGameFound()
    {
        const bool muted = false;
        SetUp_GameStateStore(false);

        var (outcome, message) = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        outcome.Should().Be(SetMuteAllPlayersOutcome.GameDoesNotExistError);
        message.Should().Be($"Couldn't find game with id: {GameId}");
    }


    [TestMethod]
    public async Task SetMuteAllPlayersAsync_ReturnsError_WhenGuildInvalid()
    {
        const bool muted = false;
        SetUp_GameStateStore(true, guildId: "invalid");

        var (outcome, message) = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        outcome.Should().Be(SetMuteAllPlayersOutcome.InvalidGuildError);
        message.Should().Be("GameState contained a guildId that is not valid");
    }

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_ReturnsError_WhenGuildNotFound()
    {
        const bool muted = false;
        SetUp_GameStateStore(true);
        SetUp_Bot(false);

        var (outcome, message) = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        outcome.Should().Be(SetMuteAllPlayersOutcome.InvalidGuildError);
        message.Should().Be("GameState contained a guildId that could not be found");
    }

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_CallsGetGuildUsers_WithOnlyStorytellers()
    {
        string[] userIds = ["user1", "user2", "user3"];
        const bool muted = false;
        SetUp_GameStateStore(true, users:
        [
            new ValueTuple<string, bool, bool, UserType>(userIds[0], true, !muted, UserType.StoryTeller),
            new ValueTuple<string, bool, bool, UserType>(userIds[1], true, !muted, UserType.Player),
            new ValueTuple<string, bool, bool, UserType>(userIds[2], true, !muted, UserType.StoryTeller)
        ]);
        SetUp_Bot(true);

        _ = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        _guild.Verify(o => o.GetGuildUsers(new[] { userIds[0], userIds[2] }), Times.Once);
    }

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_CallsGetGuildUsers_WithOnlyPresent()
    {
        string[] userIds = ["user1", "user2"];
        const bool muted = false;
        SetUp_GameStateStore(true, users:
        [
            new ValueTuple<string, bool, bool, UserType>(userIds[0], false, !muted, UserType.StoryTeller),
            new ValueTuple<string, bool, bool, UserType>(userIds[1], true, !muted, UserType.StoryTeller)
        ]);
        SetUp_Bot(true);

        _ = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        _guild.Verify(o => o.GetGuildUsers(new[] { userIds[1] }), Times.Once);
    }

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_CallsGetGuildUsers_WithOnlyUsersThatNeedMuteChange()
    {
        string[] userIds = ["user1", "user2"];
        const bool muted = false;
        SetUp_GameStateStore(true, users:
        [
            new ValueTuple<string, bool, bool, UserType>(userIds[0], true, !muted, UserType.StoryTeller),
            new ValueTuple<string, bool, bool, UserType>(userIds[1], true, muted, UserType.StoryTeller)
        ]);
        SetUp_Bot(true);

        _ = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        _guild.Verify(o => o.GetGuildUsers(new[] { userIds[0] }), Times.Once);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task SetMuteAllPlayersAsync_CallsMutesEachUser(bool muted)
    {
        string[] userIds = ["user1", "user2", "user3"];
        SetUp_GameStateStore(true, muted, users:
        [
            new ValueTuple<string, bool, bool, UserType>(userIds[0], true, !muted, UserType.StoryTeller),
            new ValueTuple<string, bool, bool, UserType>(userIds[1], true, !muted, UserType.StoryTeller),
            new ValueTuple<string, bool, bool, UserType>(userIds[2], true, !muted, UserType.StoryTeller)
        ]);
        SetUp_Bot(true);
        var user1 = new Mock<IDiscordGuildUser>();
        var user2 = new Mock<IDiscordGuildUser>();
        var user3 = new Mock<IDiscordGuildUser>();
        _guild.Setup(o => o.GetGuildUsers(userIds)).Returns([user1.Object, user2.Object, user3.Object]);

        var (outcome, message) = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        user1.Verify(o => o.SetIsServerMuted(muted), Times.Once);
        user2.Verify(o => o.SetIsServerMuted(muted), Times.Once);
        user3.Verify(o => o.SetIsServerMuted(muted), Times.Once);

        var mutedString = muted ? "Muted" : "UnMuted";
        outcome.Should().Be(SetMuteAllPlayersOutcome.PlayersUpdated);
        message.Should().Be($"{mutedString}: {userIds.Length}");
        
        
        capturedPredicate.Should().NotBeNull();
        var defaultUser = CommonMethods.GetRandomTownUser();

        var  presentMutedUser = defaultUser with { IsPresent = true, VoiceState = new VoiceState(true, false, false ,false) };
        var presentUnmutedUser =  defaultUser with { IsPresent = true, VoiceState = new VoiceState(false, false, false ,false) };
        var absentUser =  defaultUser with { IsPresent = false, VoiceState = new VoiceState(false, false, false ,false) };

        if (muted)
        {
            capturedPredicate(presentMutedUser).Should().BeFalse("present muted user should be filtered out when checkMuted=true");
            capturedPredicate(presentUnmutedUser).Should().BeTrue("present unmuted user should pass when checkMuted=true");
        }
        else
        {
            capturedPredicate(presentMutedUser).Should().BeTrue("present muted user should pass when checkMuted=false");
            capturedPredicate(presentUnmutedUser).Should().BeFalse("present unmuted user should be filtered out when checkMuted=false");
        }

        capturedPredicate(absentUser).Should().BeFalse("absent user should always be filtered out");
    }
}