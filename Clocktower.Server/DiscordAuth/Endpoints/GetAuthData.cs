using Clocktower.Server.DiscordTown.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clocktower.Server.DiscordAuth.Endpoints;

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
    private static Results<Ok<MiniUser>, NotFound> Handle(
        string key,
        [FromServices] DiscordAuthService discordAuthService)
    {
        var miniUser = discordAuthService.GetAuthData(key);
        return miniUser != null ? TypedResults.Ok(miniUser) : TypedResults.NotFound();
    }
}