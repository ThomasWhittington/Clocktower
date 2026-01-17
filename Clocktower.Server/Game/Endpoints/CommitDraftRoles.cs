namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class CommitDraftRoles : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/commit-draft-roles", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<CommitDraftRoles>()
        .WithSummary("Commits draft roles")
        .WithDescription("Commits the draft roles for a user in a game, moving them to the role fields.")
        .WithRequestValidation<GameIdRequest>();


    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.CommitDraftRoles(request.GameId);
        return result.ToHttpResult();
    }
}