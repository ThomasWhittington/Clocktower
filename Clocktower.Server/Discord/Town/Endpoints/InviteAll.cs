using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class InviteAll : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/invite-all", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<InviteAll>()
        .WithSummaryAndDescription("Invites all users to the specified game")
        .WithRequestValidation<GameIdRequest>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IDiscordTownService discordTownService)
    {
        var gameId = request.GameId.Trim();
        bool sendInvite = !gameId.StartsWith("test");
        var result = await discordTownService.InviteAll(request.GameId, sendInvite);
        return result.ToHttpResult();
    }
}