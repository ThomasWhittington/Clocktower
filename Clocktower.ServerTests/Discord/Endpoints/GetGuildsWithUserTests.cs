using Clocktower.Server.Data;
using Clocktower.Server.Discord.Endpoints;
using Clocktower.Server.Discord.Services;
using Clocktower.Server.Discord.Town.Endpoints.Validation;

namespace Clocktower.ServerTests.Discord.Endpoints;

[TestClass]
public class GetGuildsWithUserTests
{
    private Mock<IDiscordService> _mockDiscordService = null!;
    private const string ResponseMessage = "Response";


    [TestInitialize]
    public void Setup()
    {
        _mockDiscordService = new Mock<IDiscordService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetGuildsWithUser.Map(builder);

        builder.GetEndpoint("/{userId}/guilds")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getGuildsWithUserApi")
            .ShouldHaveSummary("Gets guilds that contain user")
            .ShouldHaveDescription("Gets all guilds the bot is in that the player is also an administrator")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceGetGuildsWithUserReturnsFalse()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var request = new UserIdRequest(userId);

        var guilds = new List<MiniGuild>
        {
            new(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString()),
            new(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString())
        };

        _mockDiscordService.Setup(o => o.GetGuildsWithUser(userId)).Returns((false, guilds, ResponseMessage));

        var result = GetGuildsWithUser.Handle(request, _mockDiscordService.Object);

        _mockDiscordService.Verify(o => o.GetGuildsWithUser(userId), Times.Once);

        var rawResponse = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        rawResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        rawResponse.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceGetGuildsWithUserReturnsTrue()
    {
        var userId = CommonMethods.GetRandomSnowflakeStringId();
        var request = new UserIdRequest(userId);

        var guilds = new List<MiniGuild>
        {
            new(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString()),
            new(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString())
        };

        _mockDiscordService.Setup(o => o.GetGuildsWithUser(userId)).Returns((true, guilds, ResponseMessage));

        var result = GetGuildsWithUser.Handle(request, _mockDiscordService.Object);

        _mockDiscordService.Verify(o => o.GetGuildsWithUser(userId), Times.Once);

        var rawResponse = result.Result.Should().BeOfType<Ok<GetGuildsWithUser.Response>>().Subject;
        rawResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var response = rawResponse.Value.Should().BeOfType<GetGuildsWithUser.Response>().Subject;
        response.MiniGuilds.Should().BeEquivalentTo(guilds);
    }
}