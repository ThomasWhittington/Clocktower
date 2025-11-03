using Clocktower.Server.Discord.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class Auth : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/auth", InitiateAuth)
            .SetOpenApiOperationId("InitiateDiscordAuth")
            .WithSummary("Initiate Discord OAuth")
            .WithDescription("Redirects user to Discord for OAuth authentication");

        app.MapGet("/auth/callback", HandleCallback)
            .SetOpenApiOperationId("HandleDiscordCallback")
            .WithSummary("Handle Discord OAuth callback")
            .WithDescription("Handles the callback from Discord OAuth");

        app.MapGet("/auth/data/{key}", GetAuthData)
            .SetOpenApiOperationId("GetAuthData")
            .WithSummary("Get temporary auth data")
            .WithDescription("Retrieves temporary authentication data by key");
    }


    public record Response(ulong Id, string Name);

    private static Results<RedirectHttpResult, BadRequest<string>> InitiateAuth(DiscordAuthService discordAuthService)
    {
        var (success, authorizationUrl, message) = discordAuthService.GetAuthorizationUrl();
        return success ? TypedResults.Redirect(authorizationUrl) : TypedResults.BadRequest(message);
    }

    private static async Task<Results<RedirectHttpResult, BadRequest<string>>> HandleCallback(
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromServices] DiscordAuthService discordAuthService,
        [FromServices] IMemoryCache cache)
    {
        var (success, authResult, message) = await discordAuthService.HandleCallback(error, code);

        if (success && authResult is { User: not null })
        {
            var response = new Response(ulong.Parse(authResult.User.Id), authResult.User.Username);
            var tempKey = Guid.NewGuid().ToString();
            cache.Set($"auth_data_{tempKey}", response, TimeSpan.FromMinutes(5));

            var frontendUrl = $"http://localhost:5173/auth/callback?key={tempKey}";
            return TypedResults.Redirect(frontendUrl);
        }

        var errorUrl = $"http://localhost:5173/login?error={Uri.EscapeDataString(message)}";
        return TypedResults.Redirect(errorUrl);
    }

    private static Results<Ok<Response>, NotFound> GetAuthData(
        string key,
        [FromServices] IMemoryCache cache)
    {
        if (cache.TryGetValue($"auth_data_{key}", out var userData) && userData is Response response)
        {
            cache.Remove($"auth_data_{key}");
            return TypedResults.Ok(response);
        }

        return TypedResults.NotFound();
    }
}