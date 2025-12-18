namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetPlayerGameState : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/{gameId}/players/{userId}/state", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<GetPlayerGameState>()
            .WithSummary("Get the player game state")
            .WithDescription("Gets the game state that the provided user is allowed to see")
            .WithRequestValidation<GameAndUserRequest>();
    }

    internal static async Task<Results<Ok<GameStateDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle([AsParameters] GameAndUserRequest request, [FromServices] IGameStateService gameStateService)
    {
        var result = await gameStateService.GetPlayerGameState(request.GameId, request.UserId);
        return result.ToHttpResult();
    }
}