namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class GetGames : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetGames>()
        .WithSummaryAndDescription("Gets all games, optionally filtered by guildId");

    internal static Ok<IEnumerable<GamePerspective>> Handle([FromServices] IGamePerspectiveService gamePerspectiveService, [FromQuery] string? guildId)
    {
        var games = string.IsNullOrWhiteSpace(guildId)
            ? gamePerspectiveService.GetGames()
            : gamePerspectiveService.GetGuildGames(guildId);

        return TypedResults.Ok(games);
    }
}