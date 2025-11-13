using Microsoft.AspNetCore.Mvc;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetGames>()
        .WithSummary("Gets all games, optionally filtered by guildId");

    private static Results<Ok<IEnumerable<GameState>>, NotFound<string>> Handle(GameStateService gameStateService, [FromQuery] string? guildId)
    {
        var games = string.IsNullOrWhiteSpace(guildId)
            ? gameStateService.GetGames()
            : gameStateService.GetGames(guildId);

        return TypedResults.Ok(games);
    }
}