using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class ToggleStoryTeller : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/{userId}", Handle)
        .WithSummary("Toggles the storyteller role for a user")
        .WithDescription("Adds or removes the storyteller role from the specified user")
        .WithRequestValidation<GuildAndUserRequest>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle(
        [AsParameters] GuildAndUserRequest request,
        DiscordService discordService)
    {
        var (success, message) = await discordService.ToggleStoryTeller(request.GuildId, request.UserId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record GuildAndUserRequest(ulong GuildId, ulong UserId);

    [UsedImplicitly]
    public class GuildAndUserRequestValidator : AbstractValidator<GuildAndUserRequest>
    {
        public GuildAndUserRequestValidator()
        {
            RuleFor(x => x.GuildId)
                .NotEmpty()
                .WithMessage("GuildId cannot be empty")
                .Must(BeValidDiscordSnowflake)
                .WithMessage("GuildId must be a valid Discord snowflake");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");
        }

        private static bool BeValidDiscordSnowflake(ulong id)
        {
            return id > 41943040000L;
        }
    }
}