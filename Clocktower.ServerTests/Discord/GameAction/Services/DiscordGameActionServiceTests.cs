using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord;
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
    private Mock<IDiscordTownManager> _mockDiscordTownManager = null!;
    private Mock<IDiscordConstantsService> _mockDiscordConstantsService = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<IDiscordGuild> _guild = null!;
    private GameState _gameState = null!;
    private Func<TownUser, bool> _capturedPredicate = null!;

    private void SetUp_Bot(bool hasGuild)
    {
        _mockBot.Setup(o => o.GetGuild(GuildId)).Returns(hasGuild ? _guild.Object : null);
    }

    private void SetUp_GameStateStore(bool hasGame, bool checkMuted = false, string? guildId = GuildId, (string userId, bool isPresent, bool muted, UserType userType)[]? users = null)
    {
        _guild.Setup(o => o.Id).Returns(guildId!);
        _gameState = CommonMethods.GetGameState(GameId, guildId);

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

            _gameState = _gameState with { Users = [.._gameState.Users, ..gameUsers] };
            _mockUserService.Setup(o => o.GetTownUsersForGameUsers(_gameState.StoryTellers, guildId!, It.IsAny<Func<TownUser, bool>>()))
                .Callback<IEnumerable<GameUser>, string, Func<TownUser, bool>>((_, _, predicate) => { _capturedPredicate = predicate; })
                .Returns(townUsers);
        }

        _mockGameStateStore.Setup(o => o.Get(GameId)).Returns(hasGame ? _gameState : null);
    }

    [TestInitialize]
    public void SetUp()
    {
        _mockBot = new Mock<IDiscordBot>();
        _mockGameStateStore = new Mock<IGameStateStore>();
        _mockUserService = new Mock<IUserService>();
        _mockDiscordTownManager = StrictMockFactory.Create<IDiscordTownManager>();
        _mockDiscordConstantsService = StrictMockFactory.Create<IDiscordConstantsService>();

        _guild = StrictMockFactory.Create<IDiscordGuild>();

        _sut = new DiscordGameActionService(
            _mockBot.Object,
            _mockGameStateStore.Object,
            _mockDiscordTownManager.Object,
            _mockDiscordConstantsService.Object,
            _mockUserService.Object
        );
    }

    #region SetMuteAllPlayersAsync

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_ReturnsError_WhenNoGameFound()
    {
        const bool muted = false;
        SetUp_GameStateStore(false);

        var result = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        result.ShouldFailWith(
            ErrorKind.NotFound,
            "game.not_found",
            $"Couldn't find game with id: {GameId}"
        );
    }


    [TestMethod]
    public async Task SetMuteAllPlayersAsync_ReturnsError_WhenGuildInvalid()
    {
        const bool muted = false;
        SetUp_GameStateStore(true, guildId: "invalid");

        var result = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SetMuteAllPlayersAsync_ReturnsError_WhenGuildNotFound()
    {
        const bool muted = false;
        SetUp_GameStateStore(true);
        SetUp_Bot(false);

        var result = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
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
        _guild.Setup(o => o.GetGuildUsers(new[] { userIds[0], userIds[2] })).Returns([]);

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
        _guild.Setup(o => o.GetGuildUsers(new[] { userIds[1] })).Returns([]);

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
        _guild.Setup(o => o.GetGuildUsers(new[] { userIds[0] })).Returns([]);

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

        var result = await _sut.SetMuteAllPlayersAsync(GameId, muted);

        user1.Verify(o => o.SetIsServerMuted(muted), Times.Once);
        user2.Verify(o => o.SetIsServerMuted(muted), Times.Once);
        user3.Verify(o => o.SetIsServerMuted(muted), Times.Once);

        var mutedString = muted ? "Muted" : "UnMuted";
        result.ShouldSucceedWith($"{mutedString}: {userIds.Length}");

        _capturedPredicate.Should().NotBeNull();
        var defaultUser = CommonMethods.GetRandomTownUser();

        var presentMutedUser = defaultUser with { IsPresent = true, VoiceState = new VoiceState(true, false, false, false) };
        var presentUnmutedUser = defaultUser with { IsPresent = true, VoiceState = new VoiceState(false, false, false, false) };
        var absentUser = defaultUser with { IsPresent = false, VoiceState = new VoiceState(false, false, false, false) };

        if (muted)
        {
            _capturedPredicate(presentMutedUser).Should().BeFalse("present muted user should be filtered out when checkMuted=true");
            _capturedPredicate(presentUnmutedUser).Should().BeTrue("present unmuted user should pass when checkMuted=true");
        }
        else
        {
            _capturedPredicate(presentMutedUser).Should().BeTrue("present muted user should pass when checkMuted=false");
            _capturedPredicate(presentUnmutedUser).Should().BeFalse("present unmuted user should be filtered out when checkMuted=false");
        }

        _capturedPredicate(absentUser).Should().BeFalse("absent user should always be filtered out");
    }

    #endregion

    #region SendToCottagesAsync

    private const string NightCategoryName = "Night Category";
    private List<Mock<IDiscordGuildUser>> _players = null!;
    private List<Mock<IDiscordGuildUser>> _storyTellers = null!;
    private List<Mock<IDiscordVoiceChannel>> _cottages = null!;

    private void Setup_SendToCottagesAsync(int cottageCount, (string userId, UserType userType)[]? users = null)
    {
        _players = [];
        _storyTellers = [];
        _cottages = [];
        (string userId, bool isPresent, bool muted, UserType userType)[]? gameStateUsers = null;
        if (users is not null)
        {
            gameStateUsers = [];
            gameStateUsers = users.Aggregate(gameStateUsers, (current, user) => current.Append((user.userId, true, false, user.userType)).ToArray());
        }

        SetUp_GameStateStore(true, users: gameStateUsers);
        SetUp_Bot(true);
        _guild.Setup(o => o.GetInVoiceGuildUsers(Array.Empty<string>())).Returns([]);

        if (users is not null)
        {
            var players = users.Where(o => o.userType == UserType.Player).ToArray();
            var playerIds = players.Select(o => o.userId).ToArray();
            foreach (var _ in players)
            {
                var playerMock = new Mock<IDiscordGuildUser>();
                _players.Add(playerMock);
            }

            var storyTellers = users.Where(o => o.userType == UserType.StoryTeller).ToArray();
            var storyTellerIds = storyTellers.Select(o => o.userId).ToArray();
            foreach (var _ in storyTellers)
            {
                var storyTellerMock = new Mock<IDiscordGuildUser>();
                _storyTellers.Add(storyTellerMock);
            }

            _guild.Setup(o => o.GetInVoiceGuildUsers(playerIds)).Returns(_players.Select(o => o.Object));
            _guild.Setup(o => o.GetInVoiceGuildUsers(storyTellerIds)).Returns(_storyTellers.Select(o => o.Object));
        }

        var nightChannels = new List<MiniChannel>();
        for (int i = 0; i < cottageCount; i++)
        {
            var id = i.ToString();
            nightChannels.Add(new MiniChannel(id, $"Channel {i}"));
            var cottage = new Mock<IDiscordVoiceChannel>();
            _cottages.Add(cottage);
            _guild.Setup(o => o.GetVoiceChannel(id)).Returns(cottage.Object);
        }

        _mockDiscordConstantsService.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordTownManager.Setup(o => o.GetNightChannels(GuildId, NightCategoryName)).Returns(nightChannels);
    }

    [TestMethod]
    public async Task SendToCottagesAsync_ReturnsError_WhenNoGameFound()
    {
        SetUp_GameStateStore(false);

        var result = await _sut.SendToCottagesAsync(GameId);

        result.ShouldFailWith(
            ErrorKind.NotFound,
            "game.not_found",
            $"Couldn't find game with id: {GameId}"
        );
    }

    [TestMethod]
    public async Task SendToCottagesAsync_ReturnsError_WhenGuildInvalid()
    {
        SetUp_GameStateStore(true, guildId: "invalid");

        var result = await _sut.SendToCottagesAsync(GameId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SendToCottagesAsync_ReturnsError_WhenGuildNotFound()
    {
        SetUp_GameStateStore(true);
        SetUp_Bot(false);

        var result = await _sut.SendToCottagesAsync(GameId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SendToCottagesAsync_ReturnsError_WhenNoCottagesFound()
    {
        const int cottageCount = 0;
        Setup_SendToCottagesAsync(cottageCount);

        var result = await _sut.SendToCottagesAsync(GameId);

        result.ShouldFailWith(ErrorKind.NotFound, "category.not_found");
    }

    [TestMethod]
    public async Task SendToCottagesAsync_ReturnsOk_WhenNoUsersToMove()
    {
        const int cottageCount = 3;
        Setup_SendToCottagesAsync(cottageCount, users: []);

        var result = await _sut.SendToCottagesAsync(GameId);

        result.ShouldSucceedWith("No users available to move");
    }

    [TestMethod]
    public async Task SendToCottagesAsync_ReturnsError_WhenNotEnoughCottages()
    {
        const int cottageCount = 1;
        Setup_SendToCottagesAsync(cottageCount, users:
        [
            new ValueTuple<string, UserType>("1", UserType.StoryTeller),
            new ValueTuple<string, UserType>("2", UserType.Player)
        ]);

        var result = await _sut.SendToCottagesAsync(GameId);

        result.ShouldFailWith(ErrorKind.Invalid, "channel.not_enough");
    }

    [TestMethod]
    public async Task SendToCottagesAsync_SendsUsersToCorrectChannels()
    {
        const int cottageCount = 4;
        Setup_SendToCottagesAsync(cottageCount, users:
        [
            new ValueTuple<string, UserType>("1", UserType.StoryTeller),
            new ValueTuple<string, UserType>("2", UserType.StoryTeller),
            new ValueTuple<string, UserType>("3", UserType.StoryTeller),
            new ValueTuple<string, UserType>("4", UserType.Player),
            new ValueTuple<string, UserType>("5", UserType.Player),
            new ValueTuple<string, UserType>("6", UserType.Player),
        ]);

        var result = await _sut.SendToCottagesAsync(GameId);

        for (int i = 0; i < _players.Count; i++)
        {
            var index = i;
            var player = _players[index];
            player.Verify(o => o.MoveAsync(_cottages[index + 1].Object), Times.Once);
        }

        foreach (var storyTeller in _storyTellers)
        {
            storyTeller.Verify(o => o.MoveAsync(_cottages[0].Object), Times.Once);
        }

        result.ShouldSucceedWith("Moved players to cottages");
    }

    #endregion

    #region SendToTownSquareAsync

    private const string TownSquareName = "Town Square";
    private Mock<IDiscordVoiceChannel> _townSquare = null!;
    private List<Mock<IDiscordGuildUser>> _users = null!;

    private void Setup_SendToTownSquareAsync(string? townSquareChannelId, bool channelFound, string[]? userIds = null)
    {
        SetUp_GameStateStore(true);
        SetUp_Bot(true);

        _mockDiscordTownManager.Setup(o => o.GetVoiceChannelIdByName(GuildId, TownSquareName)).Returns(townSquareChannelId);
        _mockDiscordConstantsService.Setup(o => o.NightCategoryName).Returns(NightCategoryName);
        _mockDiscordConstantsService.Setup(o => o.TownSquareName).Returns(TownSquareName);
        if (townSquareChannelId is not null)
        {
            _townSquare = new Mock<IDiscordVoiceChannel>();
            _townSquare.Setup(o => o.Id).Returns(townSquareChannelId);
            _townSquare.Setup(o => o.Name).Returns(TownSquareName);
            _guild.Setup(o => o.GetVoiceChannel(townSquareChannelId)).Returns(channelFound ? _townSquare.Object : null!);

            if (userIds is not null)
            {
                _users = [];
                var usersInChannels = new List<IDiscordGuildUser>();
                foreach (var userId in userIds)
                {
                    var mock = new Mock<IDiscordGuildUser>();
                    _users.Add(mock);
                    mock.Setup(o => o.Id).Returns(userId);
                    usersInChannels.Add(mock.Object);
                }

                _guild.Setup(o => o.GetUsersInVoiceChannelsExcluding(new[] { townSquareChannelId })).Returns(usersInChannels);
            }
        }
    }

    [TestMethod]
    public async Task SendToTownSquareAsync_ReturnsError_WhenNoGameFound()
    {
        SetUp_GameStateStore(false);

        var result = await _sut.SendToTownSquareAsync(GameId);

        result.ShouldFailWith(
            ErrorKind.NotFound,
            "game.not_found",
            $"Couldn't find game with id: {GameId}"
        );
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("invalid")]
    public async Task SendToTownSquareAsync_ReturnsError_WhenGuildInvalid(string? guildId)
    {
        SetUp_GameStateStore(true, guildId: guildId);

        var result = await _sut.SendToTownSquareAsync(GameId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SendToTownSquareAsync_ReturnsError_WhenGuildNotFound()
    {
        SetUp_GameStateStore(true);
        SetUp_Bot(false);

        var result = await _sut.SendToTownSquareAsync(GameId);

        result.ShouldFailWith(ErrorKind.Invalid, "guild.invalid_id");
    }

    [TestMethod]
    public async Task SendToTownSquareAsync_ReturnsError_WhenChannelNotFound()
    {
        Setup_SendToTownSquareAsync(null, false);

        var result = await _sut.SendToTownSquareAsync(GameId);

        result.ShouldFailWith(ErrorKind.NotFound, "channel.not_found");
    }

    [TestMethod]
    public async Task SendToTownSquareAsync_ReturnsError_WhenChannelNotInGuild()
    {
        Setup_SendToTownSquareAsync("123", false);

        var result = await _sut.SendToTownSquareAsync(GameId);

        result.ShouldFailWith(ErrorKind.NotFound, "channel.not_found");
    }


    [TestMethod]
    public async Task SendToTownSquareAsync_ReturnsOk_WhenNoUsersToMove()
    {
        Setup_SendToTownSquareAsync("123", true, userIds: []);

        var result = await _sut.SendToTownSquareAsync(GameId);

        result.ShouldSucceedWith($"No users available to move to {TownSquareName}");
    }

    [TestMethod]
    public async Task SendToTownSquareAsync_MovesUsersCorrectly()
    {
        string[] userIds = ["234", "345", "456"];
        Setup_SendToTownSquareAsync("123", true, userIds: userIds);

        var result = await _sut.SendToTownSquareAsync(GameId);

        _users.Should().HaveCount(userIds.Length);
        foreach (var user in _users)
        {
            user.Verify(o => o.MoveAsync(_townSquare.Object), Times.Once);
        }

        result.ShouldSucceedWith($"Moved all users to {TownSquareName}");
    }

    #endregion
}