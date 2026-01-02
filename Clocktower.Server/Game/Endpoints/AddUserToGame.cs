namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class AddUserToGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/add-user/{userId}", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<AddUserToGame>()
            .WithSummaryAndDescription("Adds user to the game")
            .WithRequestValidation<GameAndUserRequest>();
    }

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameAndUserRequest request,
        [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = await gamePerspectiveService.AddUserToGame(request.GameId, request.UserId);
        return result.ToHttpResult();
    }
}