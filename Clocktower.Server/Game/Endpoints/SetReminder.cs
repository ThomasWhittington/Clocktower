namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetReminder : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/set-reminder/{targetUserId}/{reminderId}", Handle)
        .SetOpenApiOperationId<SetReminder>()
        .WithSummaryAndDescription("Set a reminder for a player in a game")
        .WithRequestValidation<Request>()
        .RequireAuthorization();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        ClaimsPrincipal user,
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.SetReminder(request.GameId, user.GetUserId()!, request.TargetUserId, request.ReminderId);
        return result.ToHttpResult();
    }

    [UsedImplicitly]
    public record Request(string GameId, string TargetUserId, string ReminderId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.TargetUserId).MustBeValidSnowflake(nameof(Request.TargetUserId));
            RuleFor(x => x.ReminderId).NotEmpty();
        }
    }
}
