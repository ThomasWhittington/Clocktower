namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetGames>()
        .WithSummary("Gets all games, optionally filtered by guildId");

    internal static Ok<IEnumerable<GameState>> Handle([FromServices] IGameStateService gameStateService, [FromQuery] string? guildId)
    {
        var games = string.IsNullOrWhiteSpace(guildId)
            ? gameStateService.GetGames()
            : gameStateService.GetGuildGames(guildId);

        return TypedResults.Ok(games);
    }
}