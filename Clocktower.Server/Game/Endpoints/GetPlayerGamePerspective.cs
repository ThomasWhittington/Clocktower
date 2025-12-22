namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetPlayerGamePerspective : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/{gameId}/players/{userId}/state", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<GetPlayerGamePerspective>()
            .WithSummary("Get the player game perspective")
            .WithDescription("Gets the game perspective that the provided user is allowed to see")
            .WithRequestValidation<GameAndUserRequest>();
    }

    internal static async Task<Results<Ok<GamePerspectiveDto>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle([AsParameters] GameAndUserRequest request, [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = await gamePerspectiveService.GetPlayerGamePerspectiveDto(request.GameId, request.UserId);
        return result.ToHttpResult();
    }
}