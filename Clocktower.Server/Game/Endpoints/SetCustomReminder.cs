namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetCustomReminder : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/set-custom-reminder/{targetUserId}", Handle)
        .SetOpenApiOperationId<SetCustomReminder>()
        .WithSummaryAndDescription("Sets a custom, free-text reminder for a player in a game, visible only to the user who set it")
        .WithRequestValidation<Request>()
        .RequireAuthorization();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        ClaimsPrincipal user,
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.SetCustomReminder(request.GameId, user.GetUserId()!, request.TargetUserId, request.Body.ReminderText);
        return result.ToHttpResult();
    }

    [UsedImplicitly]
    public record Body(string ReminderText);

    [UsedImplicitly]
    public record Request(string GameId, string TargetUserId, Body Body);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.TargetUserId).MustBeValidSnowflake(nameof(Request.TargetUserId));
            RuleFor(x => x.Body.ReminderText).NotEmpty()
                .MaximumLength(40).WithMessage("ReminderText cannot be longer than 40 characters");
        }
    }
}
