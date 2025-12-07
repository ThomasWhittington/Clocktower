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

    internal static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle(string gameId, bool muted, [FromServices] IDiscordGameActionService discordGameActionService)
    {
        var (outcome, message) = await discordGameActionService.SetMuteAllPlayersAsync(gameId.Trim(), muted);

        switch (outcome)
        {
            case SetMuteAllPlayersOutcome.PlayersUpdated:
                return TypedResults.Ok(message);
            case SetMuteAllPlayersOutcome.GameDoesNotExistError:
                return TypedResults.NotFound(message);

            case SetMuteAllPlayersOutcome.InvalidGuildError:
            default:
                return TypedResults.BadRequest(message);
        }
    }
    
}