namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class AddPlayerToGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/players", Handle)
        .WithSummary("Add player to game")
        .WithRequestValidation<Request>();

    public static Results<
        Created<Player>,
        NotFound<string>,
        Conflict<string>,
        BadRequest<string>
    > Handle([AsParameters] Request request, GameStateService gameStateService)
    {
        (bool success, Player? newPlayer, string message, AddPlayerError error) = gameStateService.AddPlayerToGame(request.GameId, request.PlayerName);

        return success switch
        {
            true => TypedResults.Created($"/games/{request.GameId}/players/{newPlayer!.Id}", newPlayer),
            false when error == AddPlayerError.GameNotFound => TypedResults.NotFound(message),
            false when error == AddPlayerError.PlayerAlreadyExists => TypedResults.Conflict(message),
            false => TypedResults.BadRequest(message)
        };
    }


    [UsedImplicitly]
    public record Request(string GameId, string PlayerName);

    [UsedImplicitly]
    public record Response(int Id, string Message);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId.Trim())
                .NotEmpty().WithMessage("GameId cannot be empty");

            RuleFor(x => x.PlayerName.Trim())
                .MinimumLength(2)
                .WithMessage("Player name cannot be less than 2 characters")
                .MaximumLength(10)
                .WithMessage("Player name cannot be longer than 10 characters");
        }
    }
}