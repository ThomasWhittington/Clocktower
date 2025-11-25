using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class AddBot : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/addBot", Handle)
            .SetOpenApiOperationId<AddBot>()
            .WithSummary("Add bot to server")
            .WithDescription("Allows user to add bot to their server");
    }

    internal static Results<RedirectHttpResult, BadRequest<string>> Handle([FromServices] IDiscordAuthService discordAuthService)
    {
        var (success, url, message) = discordAuthService.GetAddBotUrl();
        return success ? TypedResults.Redirect(url) : TypedResults.BadRequest(message);
    }
}