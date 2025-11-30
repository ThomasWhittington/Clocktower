using Clocktower.Server.Discord.Auth.Endpoints;
using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.ServerTests.Discord.Auth.Endpoints;

[TestClass]
public class CallbackTests
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

        Callback.Map(builder);

        builder.GetEndpoint("/callback")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("callbackApi")
            .ShouldHaveSummaryAndDescription("Handle Discord OAuth callback");
    }

    [TestMethod]
    public async Task Handle_ReturnsRedirect_WhenServiceReturnsUrl()
    {
        var code = CommonMethods.GetRandomString();
        var error = CommonMethods.GetRandomString();
        const string url = "https://dummy.url";
        _mockDiscordAuthService.Setup(o => o.HandleCallback(error, code)).ReturnsAsync(url);

        var result = await Callback.Handle(code, error, _mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.HandleCallback(error, code), Times.Once);
        var response = result.Should().BeOfType<RedirectHttpResult>().Subject;
        response.Url.Should().Be(url);
    }
}