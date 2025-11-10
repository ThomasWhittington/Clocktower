using Clocktower.Server.Discord.Town.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clocktower.Server.Discord.Auth.Endpoints;

[UsedImplicitly]
public class GetJoinData : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/join/{key}", Handle)
            .SetOpenApiOperationId<GetJoinData>()
            .WithSummary("Get temporary join data")
            .WithDescription("Retrieves temporary join data by key");
    }

    private static Results<Ok<JoinData>, NotFound> Handle(
        string key,
        [FromServices] IDiscordTownService discordTownService)
    {
        var joinData = discordTownService.GetJoinData(key);
        return joinData != null ? TypedResults.Ok(joinData) : TypedResults.NotFound();
    }
}