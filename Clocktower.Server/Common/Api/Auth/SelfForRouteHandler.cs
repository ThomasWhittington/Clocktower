using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Clocktower.Server.Common.Api.Auth;

public class SelfForRouteHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<SelfForRouteRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SelfForRouteRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return Task.CompletedTask;

        if (context.User.HasClaim("test_bypass", "true"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var routeUserId = httpContext.Request.RouteValues["userId"]?.ToString();
        if (string.IsNullOrEmpty(routeUserId)) return Task.CompletedTask;

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Task.CompletedTask;

        if (string.Equals(userId, routeUserId, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
