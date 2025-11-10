using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class InviteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/invite/{userId}", Handle)
        .SetOpenApiOperationId<InviteUser>()
        .WithSummary("Invites user to the specified guild")
        .WithDescription("Invites user to the specified guild")
        .WithRequestValidation<GuildAndUserRequest>();

    private static async Task<Results<Ok<bool>, BadRequest<string>>> Handle(
        [AsParameters] GuildAndUserRequest request,
        IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);
        var userId = ulong.Parse(request.UserId);

        var (success, message) = await discordTownService.InviteUser(guildId, userId);
        if (success)
        {
            return TypedResults.Ok(success);
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