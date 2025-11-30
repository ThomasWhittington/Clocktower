using Clocktower.Server.Discord.Town.Endpoints;
using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.ServerTests.Discord.Town.Endpoints;

[TestClass]
public class PingUserTests
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

        PingUser.Map(builder);

        builder.GetEndpoint("/ping/{userId}")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldHaveOperationId("pingUserApi")
            .ShouldHaveSummary("Pings user")
            .ShouldHaveDescription("Sends a ping to the user if online")
            .ShouldHaveValidation();
    }

    [TestMethod]
    public async Task Handle_ReturnsOk_WhenServiceToggleStoryTellerReturnsTrue()
    {
        var request = new UserIdRequest(CommonMethods.GetRandomSnowflakeStringId());
     
        var result = await PingUser.Handle(request, _mockDiscordTownService.Object);

        _mockDiscordTownService.Verify(o => o.PingUser(request.UserId.Trim()), Times.Once);
        var response = result.Should().BeOfType<Ok>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }
}