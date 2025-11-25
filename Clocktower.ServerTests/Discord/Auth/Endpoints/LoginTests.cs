using Clocktower.Server.Discord.Auth.Endpoints;
using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.ServerTests.Discord.Auth.Endpoints;

[TestClass]
public class LoginTests
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

        Login.Map(builder);

        var endpoint = builder.GetEndpoint("/");
        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("loginApi");
        endpoint.ShouldHaveSummary("Initiate Discord OAuth");
        endpoint.ShouldHaveDescription("Redirects user to Discord for OAuth authentication");
    }

    [TestMethod]
    public void Handle_ReturnsRedirectWhenServiceReturnsTrue()
    {
        const string url = "https://dummy.url";
        const string message = "dummy message";
        _mockDiscordAuthService.Setup(o => o.GetAuthorizationUrl()).Returns((true, url, message));

        var result = Login.Handle(_mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.GetAuthorizationUrl(), Times.Once);
        var response = result.Result.Should().BeOfType<RedirectHttpResult>().Subject;
        response.Url.Should().Be(url);
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string message = "dummy message";
        _mockDiscordAuthService.Setup(o => o.GetAuthorizationUrl()).Returns((false, string.Empty, message));

        var result = Login.Handle(_mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.GetAuthorizationUrl(), Times.Once);
        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(message);
    }
}