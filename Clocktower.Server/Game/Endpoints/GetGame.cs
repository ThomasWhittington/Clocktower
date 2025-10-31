using Clocktower.Server.Data;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{gameId}", Handle)
        .SetOpenApiOperationId<GetGame>()
        .WithSummary("Get the game state by id");

    private static Results<Ok<GameState>, NotFound<string>> Handle(string gameId, GameStateService gameStateService)
    {
        var result = gameStateService.GetGame(gameId.Trim());

        return result.success
            ? TypedResults.Ok(result.gameState)
            : TypedResults.NotFound(result.message);
    }
}