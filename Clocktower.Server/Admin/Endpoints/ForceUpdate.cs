using Clocktower.Server.Admin.Services;

namespace Clocktower.Server.Admin.Endpoints;

[UsedImplicitly]
public class ForceUpdate : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/force-update/{gameId}", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<ForceUpdate>()
        .WithSummaryAndDescription("Forces a game update to be sent to all users")
        .WithRequestValidation<GameIdRequest>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameIdRequest gameIdRequest,
        [FromServices] IAdminService adminService
    )
    {
        var result = await adminService.ForceUpdate(gameIdRequest.GameId);
        return result.ToHttpResult();
    }
}