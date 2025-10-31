using Clocktower.Server.Data;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetGames>()
        .WithSummary("Gets all games");

    private static Results<Ok<GameState[]>, NotFound<string>> Handle(GameStateService gameStateService)
    {
        return TypedResults.Ok(gameStateService.GetGames());
    }
}