using Clocktower.Server.Data.Dto;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetAvailableGameUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{gameId}/available-users", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<GetAvailableGameUsers>()
            .WithSummary("Get available guild users for a game")
            .WithDescription("Gets all users that are in the games guild but are not yet added to the game")
            .WithRequestValidation<GameIdRequest>();
    }

    internal static Results<Ok<IEnumerable<UserDto>>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = gamePerspectiveService.GetAvailableGameUsers(request.GameId);
        return result.ToHttpResult();
    }
}