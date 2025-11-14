namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class StartGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/start/{guildId}/{userId}", Handle)
        .SetOpenApiOperationId<StartGame>()
        .WithSummary("Starts new game state for id")
        .WithRequestValidation<Request>();

    private static Results<Created<GameState>, BadRequest<string>> Handle([AsParameters] Request request, GameStateService gameStateService)
    {
        var gameId = request.GameId.Trim();
        var userId = ulong.Parse(request.UserId);

        var result = gameStateService.StartNewGame(request.GuildId, gameId, userId);

        return result.success
            ? TypedResults.Created($"/games/{result.gameState!.Id}", result.gameState)
            : TypedResults.BadRequest(result.message);
    }

    [UsedImplicitly]
    public record Request(string GameId, string GuildId, string UserId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId.Trim())
                .MinimumLength(3).WithMessage("GameId cannot be less than 3 characters")
                .MaximumLength(32).WithMessage("GameId cannot be longer than 32 characters");

            RuleFor(x => x.GuildId)
                .NotEmpty()
                .WithMessage("GuildId cannot be empty")
                .Must(Validation.BeValidDiscordSnowflake)
                .WithMessage("GuildId must be a valid Discord snowflake");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");
        }
    }
}