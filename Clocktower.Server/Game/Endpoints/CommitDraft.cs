namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class CommitDraft : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/commit-draft", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<CommitDraft>()
        .WithSummary("Commit draft")
        .WithDescription("Commits the draft for all users in a game, moving them to the live fields.")
        .WithRequestValidation<GameIdRequest>();


    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.CommitDraft(request.GameId);
        return result.ToHttpResult();
    }
}