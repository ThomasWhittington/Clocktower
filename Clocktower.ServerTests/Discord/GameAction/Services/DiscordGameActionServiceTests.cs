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
    private Mock<IDiscordGuild> _guild = null!;
    private GameState _gameState = null!;

    private void SetUp_Bot(bool hasGuild)
    {
        _mockBot.Setup(o => o.GetGuild(ulong.Parse(GuildId))).Returns(hasGuild ? _guild.Object : null);
    }

    private void SetUp_GameStateStore(bool hasGame, string guildId = GuildId, (string userId, bool isPresent, bool muted, UserType userType)[]? users = null)
    {
        _gameState.Id = GameId;
        _gameState.GuildId = guildId;
        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(hasGame ? _gameState : null);

        if (users is not null)
        {
            foreach (var user in users)
            {
                _gameState.Users.Add(new GameUser(user.userId, user.userId, string.Empty)
                {
                    UserType = user.userType,
                    IsPresent = user.isPresent,
                    VoiceState = new VoiceState(user.muted, false, false, false)
                });
            }
        }
    }

    [TestInitialize]
    public void SetUp()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockGameStateStore = new Mock<IGameStateStore>();
        _gameState = new GameState();
        _guild = new Mock<IDiscordGuild>();

        _sut = new DiscordGameActionService(_mockBot.Object, _mockGameStateStore.Object);
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
        SetUp_GameStateStore(true, "invalid");

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
    [DataRow (true)]
    [DataRow (false)]
    public async Task SetMuteAllPlayersAsync_CallsMutesEachUser(bool muted )
    {
        string[] userIds = ["user1", "user2", "user3"];
        SetUp_GameStateStore(true, users:
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
        
        var(outcome, message) = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        user1.Verify(o=>o.SetIsServerMuted(muted),Times.Once);
        user2.Verify(o=>o.SetIsServerMuted(muted),Times.Once);
        user3.Verify(o=>o.SetIsServerMuted(muted),Times.Once);
        
        var mutedString = muted ? "Muted" : "UnMuted";
        outcome.Should().Be(SetMuteAllPlayersOutcome.PlayersUpdated);
        message.Should().Be($"{mutedString}: {userIds.Length}");
    }
}