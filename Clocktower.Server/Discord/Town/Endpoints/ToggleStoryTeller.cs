using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class ToggleStoryTeller : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/{userId}", Handle)
        .SetOpenApiOperationId<ToggleStoryTeller>()
        .WithSummary("Toggles the storyteller role for a user")
        .WithDescription("Adds or removes the storyteller role from the specified user")
        .WithRequestValidation<GuildAndUserRequest>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle(
        [AsParameters] GuildAndUserRequest request,
        DiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);
        var userId = ulong.Parse(request.UserId);
       
        var (success, message) = await discordTownService.ToggleStoryTeller(guildId, userId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record GuildAndUserRequest(string GuildId, string UserId);

    [UsedImplicitly]
    public class GuildAndUserRequestValidator : AbstractValidator<GuildAndUserRequest>
    {
        public GuildAndUserRequestValidator()
        {
            RuleFor(x => x.GuildId)
                .NotEmpty()
                .WithMessage("GuildId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("GuildId must be a valid Discord snowflake");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");
        }
    }
}