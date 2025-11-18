namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetPlayerGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/player/{userId}", Handle)
        .SetOpenApiOperationId<GetPlayerGames>()
        .WithSummary("Gets games the player is in");

    private static Ok<IEnumerable<MiniGameState>> Handle(string userId, GameStateService gameStateService)
    {
        var result = gameStateService.GetPlayerGames(userId.Trim());
        return TypedResults.Ok(result);
    }
}