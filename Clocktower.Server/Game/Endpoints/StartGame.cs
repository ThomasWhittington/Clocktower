using Clocktower.Server.Data;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class StartGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/start", Handle)
        .SetOpenApiOperationId<StartGame>()
        .WithSummary("Starts new game state for id")
        .WithRequestValidation<Request>();

    private static Results<Created<GameState>, BadRequest<string>> Handle([AsParameters] Request request, GameStateService gameStateService)
    {
        var gameId = request.GameId.Trim();
        var result = gameStateService.StartNewGame(gameId);

        return result.success
            ? TypedResults.Created($"/games/{gameId}", result.gameState)
            : TypedResults.BadRequest(result.message);
    }

    [UsedImplicitly]
    public record Request(string GameId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId.Trim())
                .MinimumLength(3).WithMessage("GameId cannot be less than 3 characters")
                .MaximumLength(10).WithMessage("GameId cannot be longer than 10 characters");
        }
    }
}