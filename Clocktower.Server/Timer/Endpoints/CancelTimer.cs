using Clocktower.Server.Timer.Services;

namespace Clocktower.Server.Timer.Endpoints;

[UsedImplicitly]
public class CancelTimer : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{gameId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<CancelTimer>()
        .WithSummaryAndDescription("Cancels the timer for game");

    internal static async Task<Results<Ok<TimerState>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(string gameId, [FromServices] ITimerService timerService, CancellationToken ct)
    {
        var result = await timerService.CancelTimer(gameId, ct);
        return result.ToHttpResult();
    }
}