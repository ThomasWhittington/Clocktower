using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Clocktower.Server.Common.Api.Auth;

public class StoryTellerForGameHandler(
    IHttpContextAccessor httpContextAccessor,
    IGameAuthorizationService gameAuthService) : AuthorizationHandler<StoryTellerForGameRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StoryTellerForGameRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return Task.CompletedTask;

        var routeGameId = httpContext.Request.RouteValues["gameId"]?.ToString();
        if (string.IsNullOrEmpty(routeGameId)) return Task.CompletedTask;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Task.CompletedTask;

        var isStoryteller = context.User.HasClaim("is_storyteller", "true") || context.User.IsInRole("Storyteller");
        if (!isStoryteller) return Task.CompletedTask;

        var allowed = gameAuthService.IsStoryTellerForGame(userId, routeGameId);

        if (allowed) context.Succeed(requirement);

        return Task.CompletedTask;
    }
}