using Clocktower.Server.Discord.Services;
using Microsoft.AspNetCore.Mvc;

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

    private static Results<RedirectHttpResult, BadRequest<string>> InitiateAuth(DiscordAuthService discordAuthService)
    {
        var (success, authorizationUrl, message) = discordAuthService.GetAuthorizationUrl();
        return success ? TypedResults.Redirect(authorizationUrl) : TypedResults.BadRequest(message);
    }

    private static async Task<Results<RedirectHttpResult, BadRequest<string>>> HandleCallback(
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromServices] DiscordAuthService discordAuthService)
    {
        var redirectUrl = await discordAuthService.HandleCallback(error, code);
        return TypedResults.Redirect(redirectUrl);
    }

    private static Results<Ok<MiniUser>, NotFound> GetAuthData(
        string key,
        [FromServices] DiscordAuthService discordAuthService)
    {
        var miniUser = discordAuthService.GetAuthData(key);
        return miniUser != null ? TypedResults.Ok(miniUser) : TypedResults.NotFound();
    }
}