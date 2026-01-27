namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetDraftRole : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/set-draft-role/{targetUserId}/{roleId?}", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<SetDraftRole>()
        .WithSummaryAndDescription("Sets the draft role for the target user")
        .WithRequestValidation<Request>();


    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.SetDraftRole(request.GameId, request.TargetUserId, request.RoleId);
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