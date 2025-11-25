namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class StartGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/start/{guildId}/{userId}", Handle)
        .SetOpenApiOperationId<StartGame>()
        .WithSummaryAndDescription("Starts new game state for id")
        .WithRequestValidation<Request>();

    internal static Results<Created<GameState>, BadRequest<string>> Handle([AsParameters] Request request, [FromServices] IGameStateService gameStateService)
    {
        var gameId = request.GameId.Trim();
        var userId = ulong.Parse(request.UserId);

        var result = gameStateService.StartNewGame(request.GuildId, gameId, userId);

        return result.success ? TypedResults.Created($"/games/{result.gameState!.Id}", result.gameState) : TypedResults.BadRequest(result.message);
    }

    [UsedImplicitly]
    public record Request(string GameId, string GuildId, string UserId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.GuildId).MustBeValidSnowflake(nameof(Request.GuildId));
            RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(Request.UserId));
        }
    }
}