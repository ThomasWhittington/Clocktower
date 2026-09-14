using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class GetGuildsWithUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/guilds", Handle)
        .SetOpenApiOperationId<GetGuildsWithUser>()
        .WithSummary("Gets guilds that contain user")
        .WithDescription("Gets all guilds the bot is in that the player is also an administrator")
        .RequireAuthorization();

    internal static Results<Ok<Response>, BadRequest<string>> Handle(ClaimsPrincipal user, [FromServices] IDiscordService discordService)
    {
        var (success, guilds, message) = discordService.GetGuildsWithUser(user.GetUserId()!);
        return success ? TypedResults.Ok(new Response(guilds)) : TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record Response(List<MiniGuild> MiniGuilds);
}
