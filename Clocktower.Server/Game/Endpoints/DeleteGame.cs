namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class DeleteGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{gameId}/delete", Handle)
        .SetOpenApiOperationId<DeleteGame>()
        .WithSummaryAndDescription("Deletes a game by id");


    internal static Results<Ok, NotFound<string>> Handle(string gameId, [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = gamePerspectiveService.DeleteGame(gameId.Trim());

        return result.success
            ? TypedResults.Ok()
            : TypedResults.NotFound(result.message);
    }
}