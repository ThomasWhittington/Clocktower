namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class RemoveReminder : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/remove-reminder/{userId}/{targetUserId}/{reminderId}", Handle)
        .SetOpenApiOperationId<RemoveReminder>()
        .WithSummaryAndDescription("Removes a reminder for a player in a game")
        .WithRequestValidation<Request>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.RemoveReminder(request.GameId, request.UserId, request.TargetUserId, request.ReminderId);
        return result.ToHttpResult();
    }

    [UsedImplicitly]
    public record Request(string GameId, string UserId, string TargetUserId, string ReminderId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(Request.UserId));
            RuleFor(x => x.TargetUserId).MustBeValidSnowflake(nameof(Request.TargetUserId));
            RuleFor(x => x.ReminderId).NotEmpty();
        }
    }
}