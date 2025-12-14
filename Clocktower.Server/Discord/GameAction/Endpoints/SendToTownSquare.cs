using Clocktower.Server.Discord.GameAction.Services;

namespace Clocktower.Server.Discord.GameAction.Endpoints;

[UsedImplicitly]
public class SendToTownSquare : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/send-to-townsquare/{gameId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<SendToTownSquare>()
        .WithSummaryAndDescription("Sends all users to townsquare")
        .WithRequestValidation<GameIdRequest>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle([AsParameters] GameIdRequest request, [FromServices] IDiscordGameActionService discordGameActionService)
    {
        var result = await discordGameActionService.SendToTownSquareAsync(request.GameId);
        return result.ToHttpResult();
    }
}