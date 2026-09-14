using System.Security.Claims;

namespace Clocktower.Server.Common.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public string? GetUserId() => user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
