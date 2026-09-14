namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetPerspectiveRole : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/set-perspective-role/{targetUserId}/{roleId?}", Handle)
        .SetOpenApiOperationId<SetPerspectiveRole>()
        .WithSummaryAndDescription("Sets the role for the target user on the users perspective of the game")
        .WithRequestValidation<Request>()
        .RequireAuthorization();


    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        ClaimsPrincipal user,
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.SetPerspectiveRole(request.GameId, user.GetUserId()!, request.TargetUserId, request.RoleId);
        return result.ToHttpResult();
    }


    [UsedImplicitly]
    public record Request(string GameId, string TargetUserId, string? RoleId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.TargetUserId).MustBeValidSnowflake(nameof(Request.TargetUserId));
        }
    }
}
