using Clocktower.Server.Discord.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class BotCallback : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/bot-callback", Handle)
            .SetOpenApiOperationId<BotCallback>()
            .WithSummary("Handle Discord OAuth callback")
            .WithDescription("Handles the callback from Discord OAuth");
    }

    private static Results<RedirectHttpResult, BadRequest<string>> Handle(
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromQuery(Name = "guild_id")] string? guildId,
        [FromServices] DiscordAuthService discordAuthService)
    {
        var redirectUrl = discordAuthService.HandleBotCallback(error, code, guildId);
        return TypedResults.Redirect(redirectUrl);
    }
}