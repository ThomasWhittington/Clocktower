using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class StartGame : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/start/{guildId}", Handle)
        .SetOpenApiOperationId<StartGame>()
        .WithSummaryAndDescription("Starts new game perspective for id")
        .WithRequestValidation<Request>()
        .RequireAuthorization();

    internal static async Task<Results<Created<GamePerspective>, BadRequest<string>>> Handle(ClaimsPrincipal user, [AsParameters] Request request, [FromServices] IGameService gameService, [FromServices] IDiscordTownService discordTownService, [FromServices] ILogger<StartGame> logger)
    {
        var result = gameService.StartNewGame(request.GuildId, user.GetUserId()!);
        var (townSuccess, _, townMessage) = await discordTownService.GetDiscordTown(request.GuildId);
        if (!townSuccess) logger.LogWarning("Failed to fetch Discord town: {Message}", townMessage);
        return result.success ? TypedResults.Created($"/games/{result.gamePerspective!.Id}", result.gamePerspective) : TypedResults.BadRequest(result.message);
    }

    [UsedImplicitly]
    public record Request(string GuildId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GuildId).MustBeValidSnowflake(nameof(Request.GuildId));
        }
    }
}
