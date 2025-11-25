using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class Login : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/", Handle)
            .SetOpenApiOperationId<Login>()
            .WithSummary("Initiate Discord OAuth")
            .WithDescription("Redirects user to Discord for OAuth authentication");
    }

    internal static Results<RedirectHttpResult, BadRequest<string>> Handle([FromServices] IDiscordAuthService discordAuthService)
    {
        var (success, authorizationUrl, message) = discordAuthService.GetAuthorizationUrl();
        return success ? TypedResults.Redirect(authorizationUrl) : TypedResults.BadRequest(message);
    }
}