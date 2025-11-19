namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class LoadDummyGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/load", Handle)
        .SetOpenApiOperationId<LoadDummyGames>()
        .WithSummary("Loads dummy data from saved json file");

    internal static Results<Ok<string>, BadRequest<string>> Handle([FromServices] IGameStateService gameStateService)
    {
        var result = gameStateService.LoadDummyData();

        return result.success
            ? TypedResults.Ok(result.message)
            : TypedResults.BadRequest(result.message);
    }
}