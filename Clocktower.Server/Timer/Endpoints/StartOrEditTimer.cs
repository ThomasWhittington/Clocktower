using Clocktower.Server.Timer.Services;

namespace Clocktower.Server.Timer.Endpoints;

[UsedImplicitly]
public class StartOrEditTimer : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<StartOrEditTimer>()
        .WithSummaryAndDescription("Start or edit the timer for a game")
        .WithRequestValidation<Request>();

    internal static async Task<Results<Ok<TimerState>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(string gameId, [FromBody] Request request, [FromServices] ITimerService timerService, CancellationToken ct)
    {
        var result = await timerService.StartOrEditAsync(gameId, request.DurationSeconds, request.Label,ct);
        return result.ToHttpResult();
    }

    [UsedImplicitly]
    public sealed record Request(int DurationSeconds, string? Label);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.DurationSeconds).NotNull().GreaterThan(0);
        }
    }
}