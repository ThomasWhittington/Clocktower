using Clocktower.Server.Admin.Endpoints;
using Clocktower.Server.Admin.Services;

namespace Clocktower.ServerTests.Admin.Endpoints;

[TestClass]
public class GetAuthTokenTests
{
    private Mock<IAdminService> _mockAdminService = null!;

    [TestInitialize]
    public void SetUp()
    {
        _mockAdminService = new Mock<IAdminService>();
    }

    [TestMethod]
    public void Map_RegistersCorrectly()
    {
        var builder = EndpointFactory.CreateBuilder();

        GetAuthToken.Map(builder);

        builder.GetEndpoint("/auth/token")
            .ShouldHaveMethod(HttpMethod.Post)
            .ShouldAllowAnonymous()
            .ShouldHaveOperationId("getAuthTokenApi")
            .ShouldHaveSummary("Get JWT token for testing")
            .ShouldHaveDescription("Returns a JWT token for API testing purposes");
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenUsernameEmpty()
    {
        const string userName = "";
        var request = new GetAuthToken.TokenRequest(userName);

        var result = GetAuthToken.Handle(request, _mockAdminService.Object);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be("Username is required");
    }

    [TestMethod]
    public void Handle_ReturnsBadRequest_WhenServiceReturnsFalse()
    {
        const string responseMessage = "response message";
        const string userName = "username";
        var request = new GetAuthToken.TokenRequest(userName);
        _mockAdminService.Setup(o => o.GenerateJwtToken(userName)).Returns((false, responseMessage));

        var result = GetAuthToken.Handle(request, _mockAdminService.Object);

        var response = result.Result.Should().BeOfType<BadRequest<string>>().Subject;
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Value.Should().Be(responseMessage);
    }

    [TestMethod]
    public void Handle_ReturnsToken_WhenServiceReturnsTrue()
    {
        const string token = "jwt token";
        const string userName = "username";
        var request = new GetAuthToken.TokenRequest(userName);
        _mockAdminService.Setup(o => o.GenerateJwtToken(userName)).Returns((true, token));

        var result = GetAuthToken.Handle(request, _mockAdminService.Object);

        var response = result.Result.Should().BeOfType<ContentHttpResult>().Subject;
        response.StatusCode.Should().BeNull();
        response.ResponseContent.Should().Be(token);
        response.ContentType.Should().Be("text/plain");
    }
}