using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

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

    internal static async Task<Results<Ok<JoinData>, NotFound>> Handle(
        string key,
        [FromServices] IDiscordTownService discordTownService)
    {
        var joinData = await discordTownService.GetJoinData(key);
        return joinData != null ? TypedResults.Ok(joinData) : TypedResults.NotFound();
    }
}