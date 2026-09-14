using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class MoveUserToChannelTests
{
    private Mock<IDiscordTownService> _mockDiscordTownService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordTownService = new Mock<IDiscordTownService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        MoveUserToChannel.Map(builder);

        builder.GetEndpoint("/{guildId}/{channelId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("moveUserToChannelApi")
            .ShouldHaveSummaryAndDescription("Moves the user to the specified channel")
            .ShouldHaveValidation()
            .ShouldRequireAuthenticatedUser();
    }

    [TestMethod]
    public async Task Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var user = CommonMethods.CreateClaimsPrincipal(userId);
        var request = new MoveUserToChannel.Request(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.MoveUser(request.GuildId, userId, request.ChannelId)).ReturnsAsync((false, responseMessage));

        var result = await MoveUserToChannel.Handle(user, request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.MoveUser(request.GuildId, userId, request.ChannelId), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var user = CommonMethods.CreateClaimsPrincipal(userId);
        var request = new MoveUserToChannel.Request(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.MoveUser(request.GuildId, userId, request.ChannelId)).ReturnsAsync((true, responseMessage));

        var result = await MoveUserToChannel.Handle(user, request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.MoveUser(request.GuildId, userId, request.ChannelId), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(responseMessage);
    }
}
