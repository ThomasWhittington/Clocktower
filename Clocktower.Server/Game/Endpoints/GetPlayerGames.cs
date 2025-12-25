namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetPlayerGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/player/{userId}", Handle)
        .SetOpenApiOperationId<GetPlayerGames>()
        .WithSummaryAndDescription("Gets games the player is in");

    internal static Ok<IEnumerable<MiniGamePerspective>> Handle(string userId, [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = gamePerspectiveService.GetPlayerGames(userId.Trim());
        return TypedResults.Ok(result);
    }
}