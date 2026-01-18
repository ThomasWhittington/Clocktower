namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetRole : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/set-role/{targetUserId}/{roleId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<SetRole>()
        .WithSummaryAndDescription("Sets the role for the target user on all perspectives")
        .WithRequestValidation<Request>();


    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.SetRole(request.GameId, request.TargetUserId, request.RoleId);
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