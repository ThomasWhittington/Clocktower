using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class InviteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/invite/{userId}", Handle)
        .SetOpenApiOperationId<InviteUser>()
        .WithSummary("Invites user to the specified game")
        .WithDescription("Invites user to the specified game")
        .WithRequestValidation<Request>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle(
        [AsParameters] Request request,
        IDiscordTownService discordTownService)
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
            case InviteUserOutcome.UnknownError:
            default:
                return TypedResults.BadRequest(message);
        }
    }

    [UsedImplicitly]
    public record Request(string GameId, string UserId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId)
                .NotEmpty()
                .WithMessage("GameId cannot be empty");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");
        }
    }
}