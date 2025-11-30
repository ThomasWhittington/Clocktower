using Clocktower.Server.Data;
using Clocktower.Server.Discord.Auth.Endpoints;
using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.ServerTests.Discord.Auth.Endpoints;

[TestClass]
public class GetAuthDataTests
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

        GetAuthData.Map(builder);

        builder.GetEndpoint("/data/{key}")
            .ShouldHaveMethod(HttpMethod.Get)
            .ShouldHaveOperationId("getAuthDataApi")
            .ShouldHaveSummary("Get temporary auth data")
            .ShouldHaveDescription("Retrieves temporary authentication data by key");
    }

    [TestMethod]
    public void Handle_ReturnsOk_WhenAuthDataIsFound()
    {
        var key = CommonMethods.GetRandomString();
        var user = new GameUser(CommonMethods.GetRandomString(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString());
        var authData = new UserAuthData(user, CommonMethods.GetRandomString());

        _mockDiscordAuthService.Setup(o => o.GetAuthData(key)).Returns(authData);

        var result = GetAuthData.Handle(key, _mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.GetAuthData(key), Times.Once);
        var response = result.Result.Should().BeOfType<Ok<UserAuthData>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Value.Should().Be(authData);
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenAuthDataIsNotFound()
    {
        var key = CommonMethods.GetRandomString();

        _mockDiscordAuthService.Setup(o => o.GetAuthData(key)).Returns((UserAuthData)null!);

        var result = GetAuthData.Handle(key, _mockDiscordAuthService.Object);

        _mockDiscordAuthService.Verify(o => o.GetAuthData(key), Times.Once);
        var response = result.Result.Should().BeOfType<NotFound>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }
}