namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetPlayerGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/player/{userId}", Handle)
        .SetOpenApiOperationId<GetPlayerGames>()
        .WithSummaryAndDescription("Gets games the player is in");

    internal static Ok<IEnumerable<MiniGameState>> Handle(string userId, [FromServices] IGameStateService gameStateService)
    {
        var result = gameStateService.GetPlayerGames(userId.Trim());
        return TypedResults.Ok(result);
    }
}