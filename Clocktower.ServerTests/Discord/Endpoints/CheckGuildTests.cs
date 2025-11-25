using Clocktower.Server.Discord.Endpoints;
using Clocktower.Server.Discord.Services;
using Clocktower.Server.Discord.Town.Endpoints.Validation;

namespace Clocktower.ServerTests.Discord.Endpoints;

[TestClass]
public class CheckGuildTests
{
    private Mock<IDiscordService> _mockDiscordService = null!;
    private const string ResponseName = "Name";
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

        CheckGuild.Map(builder);

        var endpoint = builder.GetEndpoint("/{guildId}/check");

        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("checkGuildApi");
        endpoint.ShouldHaveSummary("Checks access to guild");
        endpoint.ShouldHaveDescription("Checks if bot has access to the guild");
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceCheckGuildReturnsFalse()
    {
        var guildId = CommonMethods.GetRandomSnowflakeNumberId();
        var request = new GuildIdRequest(guildId.ToString());

        _mockDiscordService.Setup(o => o.CheckGuildId(guildId)).Returns((false, ResponseName, ResponseMessage));

        var result = CheckGuild.Handle(request, _mockDiscordService.Object);

        _mockDiscordService.Verify(o => o.CheckGuildId(guildId), Times.Once);

        var rawResponse = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        rawResponse.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        rawResponse.Value.Should().Be(ResponseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceGetGameReturnsTrue_Valid()
    {
        var guildId = CommonMethods.GetRandomSnowflakeNumberId();
        var request = new GuildIdRequest(guildId.ToString());

        _mockDiscordService.Setup(o => o.CheckGuildId(guildId)).Returns((true, ResponseName, ResponseMessage));

        var result = CheckGuild.Handle(request, _mockDiscordService.Object);

        _mockDiscordService.Verify(o => o.CheckGuildId(guildId), Times.Once);

        var rawResponse = result.Result.Should().BeOfType<Ok<CheckGuild.Response>>().Subject;
        rawResponse.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var response = rawResponse.Value.Should().BeOfType<CheckGuild.Response>().Subject;
        response.Valid.Should().BeTrue();
        response.Name.Should().Be(ResponseName);
        response.Message.Should().Be(ResponseMessage);
    }
}