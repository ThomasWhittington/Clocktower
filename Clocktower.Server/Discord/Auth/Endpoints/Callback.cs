using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class Callback : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/callback", Handle)
            .SetOpenApiOperationId<Callback>()
            .WithSummary("Handle Discord OAuth callback")
            .WithDescription("Handles the callback from Discord OAuth");
    }

    private static async Task<Results<RedirectHttpResult, BadRequest<string>>> Handle(
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromServices] IDiscordAuthService discordAuthService)
    {
        var redirectUrl = await discordAuthService.HandleCallback(error, code);
        return TypedResults.Redirect(redirectUrl);
    }
}