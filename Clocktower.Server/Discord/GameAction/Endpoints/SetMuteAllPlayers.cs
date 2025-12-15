using Clocktower.Server.Discord.GameAction.Services;

namespace Clocktower.Server.Discord.GameAction.Endpoints;

[UsedImplicitly]
public class SetMuteAllPlayers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/set-mute-players/{gameId}/{muted:bool}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<SetMuteAllPlayers>()
        .WithSummary("Sets muted status for players in game")
        .WithDescription("Sets muted status for players (not storytellers/ spectators) connected to voice for game");

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(string gameId, bool muted, [FromServices] IDiscordGameActionService discordGameActionService)
    {
        var result = await discordGameActionService.SetMuteAllPlayersAsync(gameId, muted);
        return result.ToHttpResult();
    }
}