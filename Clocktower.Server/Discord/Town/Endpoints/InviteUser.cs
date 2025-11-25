using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class InviteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/invite/{userId}", Handle)
        .RequireAuthorization("StoryTellerForGame")
        .SetOpenApiOperationId<InviteUser>()
        .WithSummary("Invites user to the specified game")
        .WithDescription("Invites user to the specified game")
        .WithRequestValidation<GameAndUserRequest>();

    internal static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle(
        [AsParameters] GameAndUserRequest request,
       [FromServices] IDiscordTownService discordTownService)
    {
        var gameId = request.GameId.Trim();
        var userId = ulong.Parse(request.UserId);

        var (outcome, message) = await discordTownService.InviteUser(gameId, userId);

        switch (outcome)
        {
            case InviteUserOutcome.InviteSent: return TypedResults.Ok(message);
            case InviteUserOutcome.GameDoesNotExistError:
            case InviteUserOutcome.UserNotFoundError:
                return TypedResults.NotFound(message);
            case InviteUserOutcome.InvalidGuildError:
            case InviteUserOutcome.DmChannelFailed:
            case InviteUserOutcome.UnknownError:
            default:
                return TypedResults.BadRequest(message);
        }
    }
}