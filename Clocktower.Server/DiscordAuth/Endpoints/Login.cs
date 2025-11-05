using Clocktower.Server.DiscordTown.Services;

namespace Clocktower.Server.DiscordAuth.Endpoints;

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

    private static Results<RedirectHttpResult, BadRequest<string>> Handle(DiscordAuthService discordAuthService)
    {
        var (success, authorizationUrl, message) = discordAuthService.GetAuthorizationUrl();
        return success ? TypedResults.Redirect(authorizationUrl) : TypedResults.BadRequest(message);
    }
}