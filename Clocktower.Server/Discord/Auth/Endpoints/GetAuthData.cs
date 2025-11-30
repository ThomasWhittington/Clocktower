using Clocktower.Server.Discord.Auth.Services;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class GetAuthData : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/data/{key}", Handle)
            .SetOpenApiOperationId<GetAuthData>()
            .WithSummary("Get temporary auth data")
            .WithDescription("Retrieves temporary authentication data by key");
    }

    internal static Results<Ok<UserAuthData>, NotFound> Handle(
        string key,
        [FromServices] IDiscordAuthService discordAuthService)
    {
        var gameUser = discordAuthService.GetAuthData(key);
        return gameUser != null ? TypedResults.Ok(gameUser) : TypedResults.NotFound();
    }
}