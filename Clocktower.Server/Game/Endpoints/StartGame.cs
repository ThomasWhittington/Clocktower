namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class StartGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{name}/start", Handle)
        .SetOpenApiOperationId<StartGame>()
        .WithSummary("Starts new game state for id")
        .WithRequestValidation<Request>();

    private static Results<Created<GameState>, BadRequest<string>> Handle([AsParameters] Request request, GameStateService gameStateService)
    {
        var name = request.Name.Trim();
        var result = gameStateService.StartNewGame(name);

        return result.success
            ? TypedResults.Created($"/games/{name}", result.gameState)
            : TypedResults.BadRequest(result.message);
    }

    [UsedImplicitly]
    public record Request(string Name);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name.Trim())
                .MinimumLength(3).WithMessage("Name cannot be less than 3 characters")
                .MaximumLength(32).WithMessage("Name cannot be longer than 32 characters");
        }
    }
}