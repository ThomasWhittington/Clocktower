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
        .WithRequestValidation<UserIdRequest>();

    internal static Results<Ok<Response>, BadRequest<string>> Handle([AsParameters] UserIdRequest request, [FromServices] IDiscordService discordService)
    {
        var userId = ulong.Parse(request.UserId);

        var (success, guilds, message) = discordService.GetGuildsWithUser(userId);
        return success ? TypedResults.Ok(new Response(guilds)) : TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record Response(List<MiniGuild> MiniGuilds);
}