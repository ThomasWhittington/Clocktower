namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetTime : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/time", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<SetTime>()
        .WithSummary("Sets the time of the town")
        .WithDescription("Sets the game perspective of the town based on the day time")
        .WithRequestValidation<Request>();


    internal static async Task<Results<Ok<string>, BadRequest<string>>> Handle([AsParameters] Request request, [FromServices] IGameService gameService)
    {
        var gameId = request.GameId.Trim();

        var (success, message) = await gameService.SetTime(gameId, request.GameTime);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(string GameId, GameTime GameTime);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
        }
    }
}