namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class LoadDummyGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/load", Handle)
        .SetOpenApiOperationId<LoadDummyGames>()
        .WithSummaryAndDescription("Loads dummy data from saved json file");

    internal static Results<Ok<string>, BadRequest<string>> Handle([FromServices] IGameService gameService)
    {
        var result = gameService.LoadDummyData();

        return result.success
            ? TypedResults.Ok(result.message)
            : TypedResults.BadRequest(result.message);
    }
}