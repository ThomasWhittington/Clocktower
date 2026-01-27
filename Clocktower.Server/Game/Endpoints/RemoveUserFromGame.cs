namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class RemoveUserFromGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/remove-user/{userId}", Handle)
            .RequireStorytellerForGame()
            .SetOpenApiOperationId<RemoveUserFromGame>()
            .WithSummaryAndDescription("Removes user from the game")
            .WithRequestValidation<GameAndUserRequest>();
    }

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameAndUserRequest request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.RemoveUserFromGame(request.GameId, request.UserId);
        return result.ToHttpResult();
    }
}