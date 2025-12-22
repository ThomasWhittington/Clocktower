namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetGames>()
        .WithSummaryAndDescription("Gets all games perspectives");

    internal static Ok<IEnumerable<GamePerspective>> Handle([FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var games = gamePerspectiveService.GetGames();
        return TypedResults.Ok(games);
    }
}