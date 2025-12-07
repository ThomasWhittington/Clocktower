using System.Security.Claims;
using Clocktower.Server.Common.Api;
using Clocktower.Server.Common.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Clocktower.ServerTests.Common.Api.Auth;

[TestClass]
public class StoryTellerForGameHandlerTests
{
    private StoryTellerForGameHandler Sut =>
        new(_mockHttpContextAccessor.Object, _mockGameAuthService.Object);

    private Mock<IHttpContextAccessor> _mockHttpContextAccessor = null!;
    private Mock<IGameAuthorizationService> _mockGameAuthService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockGameAuthService = new Mock<IGameAuthorizationService>();
    }


    [TestMethod]
    public async Task HandleAsync_ShouldSucceed_WhenUserIsAuthorizedStoryTeller()
    {
        const string userId = "user123";
        const string gameId = "game456";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["gameId"] = gameId;
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("is_storyteller", "true")
        ]));

        _mockGameAuthService.Setup(s => s.IsStoryTellerForGame(userId, gameId))
            .Returns(true);

        var requirement = new StoryTellerForGameRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }
    
    
    [TestMethod]
    public async Task HandleAsync_ShouldSucceed_WhenTestBypass()
    {
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("test_bypass", "true")
        ]));
        
        var requirement = new StoryTellerForGameRequirement();
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenUserIsNotStoryTellerRole()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["gameId"] = "game1";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user1")
        ]));

        var context = new AuthorizationHandlerContext([new StoryTellerForGameRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _mockGameAuthService.Verify(s => s.IsStoryTellerForGame(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenUserIdIsNotFound()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["gameId"] = "game1";
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var context = new AuthorizationHandlerContext([new StoryTellerForGameRequirement()], new ClaimsPrincipal(), null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _mockGameAuthService.Verify(s => s.IsStoryTellerForGame(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenGameAuthReturnsFalse()
    {
        const string userId = "user1";
        const string gameId = "game1";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["gameId"] = gameId;
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Storyteller")
        ]));

        _mockGameAuthService.Setup(s => s.IsStoryTellerForGame(userId, gameId))
            .Returns(false);

        var context = new AuthorizationHandlerContext([new StoryTellerForGameRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WhenRouteGameIdIsMissing()
    {
        var httpContext = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "user1"),
            new Claim(ClaimTypes.Role, "Storyteller")
        ]));

        var context = new AuthorizationHandlerContext([new StoryTellerForGameRequirement()], user, null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldNotSucceed_WheNoHttpContextProvided()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        var context = new AuthorizationHandlerContext([new StoryTellerForGameRequirement()], new ClaimsPrincipal(), null);

        await Sut.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        _mockGameAuthService.Verify(s =>
            s.IsStoryTellerForGame(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}