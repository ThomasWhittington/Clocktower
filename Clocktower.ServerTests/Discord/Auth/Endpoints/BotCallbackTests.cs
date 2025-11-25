using Clocktower.Server.Discord.Auth.Endpoints;
using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.ServerTests.Discord.Auth.Endpoints;

[TestClass]
public class BotCallbackTests
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

        BotCallback.Map(builder);

        var endpoint = builder.GetEndpoint("/bot-callback");
        endpoint.ShouldHaveMethod(HttpMethod.Get);
        endpoint.ShouldHaveOperationId("botCallbackApi");
        endpoint.ShouldHaveSummaryAndDescription("Handle Discord bot OAuth callback");
    }

    [TestMethod]
    public void Handle_ReturnsRedirect_WhenServiceReturnsUrl()
    {
        var code = CommonMethods.GetRandomString();
        var error = CommonMethods.GetRandomString();
        var guildId = CommonMethods.GetRandomSnowflakeStringId();
        const string url = "https://dummy.url";
        _mockDiscordAuthService.Setup(o => o.HandleBotCallback(error, code,guildId)).Returns(url);

        var result = BotCallback.Handle(code, error,guildId,_mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.HandleBotCallback(error, code,guildId), Times.Once);
        var response = result.Should().BeOfType<RedirectHttpResult>().Subject;
        response.Url.Should().Be(url);
    }
}