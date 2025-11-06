using Clocktower.Server.Common;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class GetGuildsWithUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{userId}/guilds", Handle)
        .SetOpenApiOperationId<GetGuildsWithUser>()
        .WithSummary("Gets guilds that contain user")
        .WithDescription("Gets all guilds the bot is in that the player is also an administrator")
        .WithRequestValidation<Request>();

    private static async Task<Results<Ok<Response>, BadRequest<string>>> Handle([AsParameters] Request request, DiscordService discordService)
    {
        var userId = ulong.Parse(request.UserId);

        var (success, guilds, message) = await discordService.GetGuildsWithUser(userId);
        if (success)
        {
            return TypedResults.Ok(new Response(guilds));
        }

        return TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record Response(List<DiscordService.MiniGuild> MiniGuilds);

    [UsedImplicitly]
    public record Request(string UserId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");
        }
    }
}