using System.Security.Claims;
using Clocktower.Server.Common.Api;
using Clocktower.Server.Common.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Clocktower.ServerTests.Common.Api.Auth;

[TestClass]
public class SelfForRouteHandlerTests
{
    private SelfForRouteHandler Sut => new(_mockHttpContextAccessor.Object);

    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldSucceed_WhenRouteUserIdMatchesClaim()
    {
        const string userId = "user123";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["userId"] = userId;
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId)
        ]));

        var context = new AuthorizationHandlerContext([new SelfForRouteRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldSucceed_WhenTestBypass()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["userId"] = "someoneElse";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("test_bypass", "true")
        ]));

        var context = new AuthorizationHandlerContext([new SelfForRouteRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenRouteUserIdDoesNotMatchClaim()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["userId"] = "someoneElse";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ]));

        var context = new AuthorizationHandlerContext([new SelfForRouteRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenCallerHasNoNameIdentifierClaim()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["userId"] = "user123";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var context = new AuthorizationHandlerContext([new SelfForRouteRequirement()], new ClaimsPrincipal(), null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenRouteUserIdIsMissing()
    {
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user123")
        ]));

        var context = new AuthorizationHandlerContext([new SelfForRouteRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenNoHttpContextProvided()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        var context = new AuthorizationHandlerContext([new SelfForRouteRequirement()], new ClaimsPrincipal(), null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
