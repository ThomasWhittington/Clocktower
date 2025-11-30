using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class GetTownStatusTests
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

        GetTownStatus.Map(builder);

        builder.GetEndpoint("/{guildId}/status")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getTownStatusApi")
            .ShouldHaveSummary("Get status of town")
            .ShouldHaveDescription("Gets if the town exists in a valid state")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());
        _mockDiscordTownService.Setup(o => o.GetTownStatus(ulong.Parse(request.GuildId))).Returns((false, false, responseMessage));

        var result =  GetTownStatus.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetTownStatus(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenServiceReturnsTrue()
    {
        const string responseMessage = "response message";
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        _mockDiscordTownService.Setup(o => o.GetTownStatus(ulong.Parse(request.GuildId))).Returns((true, true, responseMessage));

        var result =  GetTownStatus.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.GetTownStatus(ulong.Parse(request.GuildId)), Times.Once);

        var response = result.Result.Should().BeOfType<Ok<GetTownStatus.Response>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var res = response.Value.Should().BeOfType<GetTownStatus.Response>().Subject;
        res.Exists.Should().BeTrue();
        res.Message.Should().Be(responseMessage);
    }
}