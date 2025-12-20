namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{gameId}", Handle)
        .SetOpenApiOperationId<GetGame>()
        .WithSummaryAndDescription("Get the game perspective by id");

    internal static Results<Ok<IEnumerable<GamePerspective>>, NotFound<string>> Handle(string gameId, [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = gamePerspectiveService.GetGame(gameId.Trim());

        return result.success
            ? TypedResults.Ok(result.perspectives)
            : TypedResults.NotFound(result.message);
    }
}