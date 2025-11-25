using Clocktower.Server.Discord.Auth.Endpoints;
using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.ServerTests.Discord.Auth.Endpoints;

[TestClass]
public class AddBotTests
{
    private Mock<IDiscordAuthService> _mockDiscordAuthService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockDiscordAuthService = new Mock<IDiscordAuthService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        AddBot.Map(builder);

        var endpoint = builder.GetEndpoint("/addBot");
        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("addBotApi");
        endpoint.ShouldHaveSummary("Add bot to server");
        endpoint.ShouldHaveDescription("Allows user to add bot to their server");
    }

    [TestMethod]
    public void Handle_ReturnsRedirect_WhenServiceReturnsTrue()
    {
        const string url = "https://dummy.url";
        const string message = "dummy message";
        _mockDiscordAuthService.Setup(o => o.GetAddBotUrl()).Returns((true, url, message));

        var result = AddBot.Handle(_mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.GetAddBotUrl(), Times.Once);
        var response = result.Result.Should().BeOfType<RedirectHttpResult>().Subject;
        response.Url.Should().Be(url);
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string message = "dummy message";
        _mockDiscordAuthService.Setup(o => o.GetAddBotUrl()).Returns((false, string.Empty, message));

        var result = AddBot.Handle(_mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.GetAddBotUrl(), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(message);
    }
}