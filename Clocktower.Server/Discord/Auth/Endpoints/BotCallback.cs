using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class BotCallback : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/bot-callback", Handle)
            .SetOpenApiOperationId<BotCallback>()
            .WithSummary("Handle Discord bot OAuth callback");
    }

    internal static RedirectHttpResult Handle(
        [FromQuery] string? code,
        [FromQuery] string? error,
        [FromQuery(Name = "guild_id")] string? guildId,
        [FromServices] IDiscordAuthService discordAuthService)
    {
        var redirectUrl = discordAuthService.HandleBotCallback(error, code, guildId);
        return TypedResults.Redirect(redirectUrl);
    }
}