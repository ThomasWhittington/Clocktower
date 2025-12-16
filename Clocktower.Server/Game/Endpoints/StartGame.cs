using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class StartGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/start/{guildId}/{userId}", Handle)
        .SetOpenApiOperationId<StartGame>()
        .WithSummaryAndDescription("Starts new game state for id")
        .WithRequestValidation<Request>();

    internal static async Task<Results<Created<GameState>, BadRequest<string>>> Handle([AsParameters] Request request, [FromServices] IGameStateService gameStateService, [FromServices] IDiscordTownService discordTownService, [FromServices] ILogger<StartGame> logger)
    {
        var gameId = request.GameId.Trim();
        var result = gameStateService.StartNewGame(request.GuildId, gameId, request.UserId);
        var (townSuccess, _, townMessage) = await discordTownService.GetDiscordTown(request.GuildId);
        if (!townSuccess) logger.LogWarning("Failed to fetch Discord town: {Message}", townMessage);
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