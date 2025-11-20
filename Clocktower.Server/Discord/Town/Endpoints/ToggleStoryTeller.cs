using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class ToggleStoryTeller : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/{userId}", Handle)
        .SetOpenApiOperationId<ToggleStoryTeller>()
        .WithSummary("Toggles the storyteller role for a user")
        .WithDescription("Adds or removes the storyteller role from the specified user")
        .WithRequestValidation<Request>();

    private static async Task<Results<Ok<string>, BadRequest<string>>> Handle(
        [AsParameters] Request request,
        IDiscordTownService discordTownService)
    {
        var userId = ulong.Parse(request.UserId);

        var (success, message) = await discordTownService.ToggleStoryTeller(request.GameId.Trim(), userId);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
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