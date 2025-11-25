namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{gameId}", Handle)
        .SetOpenApiOperationId<GetGame>()
        .WithSummaryAndDescription("Get the game state by id");

    internal static Results<Ok<GameState>, NotFound<string>> Handle(string gameId, [FromServices] IGameStateService gameStateService)
    {
        var result = gameStateService.GetGame(gameId.Trim());

        return result.success
            ? TypedResults.Ok(result.gameState)
            : TypedResults.NotFound(result.message);
    }
}