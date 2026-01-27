using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class InviteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/invite/{userId}", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<InviteUser>()
        .WithSummaryAndDescription("Invites user to the specified game")
        .WithRequestValidation<GameAndUserRequest>();

    internal static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle(
        [AsParameters] GameAndUserRequest request,
        [FromServices] IDiscordTownService discordTownService,
        [FromServices] IConfiguration configuration)
    {
        bool sendInvite = configuration.GetValue("Discord:SendInvites", false);
        var (outcome, message) = await discordTownService.InviteUser(request.GameId, request.UserId, sendInvite);

        switch (outcome)
        {
            case InviteUserOutcome.InviteSent: return TypedResults.Ok(message);
            case InviteUserOutcome.GameDoesNotExistError:
            case InviteUserOutcome.UserNotFoundError:
                return TypedResults.NotFound(message);
            case InviteUserOutcome.InvalidGuildError:
            case InviteUserOutcome.DmChannelError:
            case InviteUserOutcome.UnknownError:
            default:
                return TypedResults.BadRequest(message);
        }
    }
}