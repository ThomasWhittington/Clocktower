using Clocktower.Server.Discord.GameAction.Services;

namespace Clocktower.Server.Discord.GameAction.Endpoints;

[UsedImplicitly]
public class SendToCottages : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/send-to-cottages/{gameId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<SendToCottages>()
        .WithSummary("Sends all users to cottages")
        .WithDescription("Sends all users to cottages and storytellers to consultation")
        .WithRequestValidation<GameIdRequest>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle([AsParameters] GameIdRequest request, [FromServices] IDiscordGameActionService discordGameActionService)
    {
        var result = await discordGameActionService.SendToCottagesAsync(request.GameId);
        return result.ToHttpResult();
    }
}