using Clocktower.Server.Common.Services;
using Clocktower.Server.Data;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Discord.Services;

namespace Clocktower.ServerTests.Discord.Services;

[TestClass]
public class DiscordServiceTests
{
    private Mock<IDiscordBot> _mockBot = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockBot = new Mock<IDiscordBot>();
    }

    private IDiscordService Sut => new DiscordService(_mockBot.Object);

    #region SendMessage

    [TestMethod]
    public async Task SendMessage_ReturnsFalse_WhenUserNotFound()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var message = CommonMethods.GetRandomString();

        _mockBot.Setup(o => o.GetUserAsync(userId)).ReturnsAsync((IDiscordUser)null!);

        var result = await Sut.SendMessage(userId, message);

        result.success.Should().BeFalse();
        result.message.Should().Be($"Couldn't find user: {userId}");
    }

    [TestMethod]
    public async Task SendMessage_ReturnsFalse_WhenDmChannelFailed()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var message = CommonMethods.GetRandomString();

        var mockUser = new Mock<IDiscordUser>();
        var mockDmChannel = new Mock<IDiscordDmChannel>();

        _mockBot.Setup(o => o.GetUserAsync(userId)).ReturnsAsync(mockUser.Object);
        mockUser.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync((IDiscordDmChannel)null!);

        var result = await Sut.SendMessage(userId, message);

        mockUser.Verify(o => o.CreateDmChannelAsync(), Times.Once);
        mockDmChannel.Verify(o => o.SendMessageAsync(message), Times.Never);

        result.success.Should().BeFalse();
        result.message.Should().Be($"Failed to create dm channel for user: {userId}");
    }

    [TestMethod]
    public async Task SendMessage_ReturnsTrue_WhenMessageIsSent()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var message = CommonMethods.GetRandomString();

        var mockUser = new Mock<IDiscordUser>();
        var mockDmChannel = new Mock<IDiscordDmChannel>();

        _mockBot.Setup(o => o.GetUserAsync(userId)).ReturnsAsync(mockUser.Object);
        mockUser.Setup(o => o.CreateDmChannelAsync()).ReturnsAsync(mockDmChannel.Object);

        var result = await Sut.SendMessage(userId, message);

        mockUser.Verify(o => o.CreateDmChannelAsync(), Times.Once);
        mockDmChannel.Verify(o => o.SendMessageAsync(message), Times.Once);

        result.success.Should().BeTrue();
        result.message.Should().Be("Sent message to user");
    }


    [TestMethod]
    public async Task SendMessage_ReturnsFalse_WhenExceptionThrown()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var message = CommonMethods.GetRandomString();

        _mockBot.Setup(o => o.GetUserAsync(userId)).Throws<Exception>();

        var result = await Sut.SendMessage(userId, message);

        result.success.Should().BeFalse();
        result.message.Should().Be("Failed to send message to user");
    }

    #endregion

    #region CheckGuildId

    [TestMethod]
    public void CheckGuildId_ReturnsFalse_WhenGuildNotFound()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        _mockBot.Setup(o => o.GetGuild(guildId)).Returns((DiscordGuild)null!);

        var result = Sut.CheckGuildId(guildId);

        result.success.Should().BeFalse();
        result.guildName.Should().BeEmpty();
        result.message.Should().Be("Bot does not have access to guild");
    }

    [TestMethod]
    public void CheckGuildId_ReturnsTrue_WhenGuildFound()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        var guildName = CommonMethods.GetRandomString();

        var mockGuild = MockMaker.CreateMockDiscordGuild(guildId, guildName);

        var mockBot = new Mock<IDiscordBot>();
        mockBot.Setup(o => o.GetGuild(guildId)).Returns(mockGuild);

        var service = new DiscordService(mockBot.Object);
        var result = service.CheckGuildId(guildId);

        result.success.Should().BeTrue();
        result.guildName.Should().Be(guildName);
        result.message.Should().Be("Bot has access to guild");
    }

    [TestMethod]
    public void CheckGuildId_ReturnsFalse_WhenExceptionThrown()
    {
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        _mockBot.Setup(o => o.GetGuild(guildId)).Throws<Exception>();

        var result = Sut.CheckGuildId(guildId);

        result.success.Should().BeFalse();
        result.guildName.Should().BeEmpty();
        result.message.Should().Be($"Bot does not have access to guild: {guildId}");
    }

    #endregion

    #region GetGuildsWithUser

    [TestMethod]
    public void GetGuildsWithUser_ReturnsEmpty_WhenNoGuildsReturned()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();

        _mockBot.Setup(o => o.GetGuilds()).Returns([]);

        var result = Sut.GetGuildsWithUser(userId);

        result.success.Should().BeTrue();
        result.guilds.Should().BeEmpty();
        result.message.Should().Be("Bot is not in any guilds");
    }

    [TestMethod]
    public void GetGuildsWithUser_ReturnsEmpty_WhenUserNotAdministrator()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var user = MockMaker.CreateMockDiscordGuildUser(userId);


        var guilds = new List<IDiscordGuild>
        {
            MockMaker.CreateMockDiscordGuild(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [user]),
            MockMaker.CreateMockDiscordGuild(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), [user])
        };

        _mockBot.Setup(o => o.GetGuilds()).Returns(guilds);

        var result = Sut.GetGuildsWithUser(userId);

        result.success.Should().BeTrue();
        result.guilds.Should().BeEmpty();
        result.message.Should().Be("Received 0 guilds where user has admin");
    }

    [TestMethod]
    public void GetGuildsWithUser_ReturnsGuilds_WhenUserIsAdministrator()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();

        var guilds = new List<IDiscordGuild>
        {
            MockMaker.CreateMockDiscordGuild("1", "guild1", [
                MockMaker.CreateMockDiscordGuildUser(userId, isAdmin: true)
            ]),
            MockMaker.CreateMockDiscordGuild("2", "guild2", [
                MockMaker.CreateMockDiscordGuildUser(userId)
            ]),
            MockMaker.CreateMockDiscordGuild("3", "guild3", [
                MockMaker.CreateMockDiscordGuildUser(userId, isAdmin: true)
            ]),
            MockMaker.CreateMockDiscordGuild("4", "guild4")
        };

        var expectedArr = new List<IDiscordGuild> { guilds[0], guilds[2] };
        var expected = expectedArr.Select(o => new MiniGuild(o.Id.ToString(), o.Name));

        _mockBot.Setup(o => o.GetGuilds()).Returns(guilds);

        var result = Sut.GetGuildsWithUser(userId);

        result.success.Should().BeTrue();
        result.guilds.Should().HaveCount(2);
        result.guilds.Should().BeEquivalentTo(expected);
        result.message.Should().Be("Received 2 guilds where user has admin");
    }

    [TestMethod]
    public void GetGuildsWithUser_ReturnsFalse_WhenExceptionThrown()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        _mockBot.Setup(o => o.GetGuilds()).Throws<Exception>();

        var result = Sut.GetGuildsWithUser(userId);

        result.success.Should().BeFalse();
        result.guilds.Should().BeEmpty();
        result.message.Should().Be("Failed to gather guilds with user");
    }

    #endregion
}