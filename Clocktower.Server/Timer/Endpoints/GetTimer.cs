using Clocktower.Server.Timer.Services;

namespace Clocktower.Server.Timer.Endpoints;

[UsedImplicitly]
public class GetTimer : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{gameId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<GetTimer>()
        .WithSummaryAndDescription("Gets the timer state for game");

    internal static Ok<TimerState> Handle(string gameId, [FromServices] ITimerService timerService)
    {
        var result = timerService.GetTimer(gameId);
        return TypedResults.Ok(result);
    }
}