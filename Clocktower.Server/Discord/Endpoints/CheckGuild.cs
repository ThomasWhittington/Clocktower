using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class CheckGuild : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/check", Handle)
        .SetOpenApiOperationId<CheckGuild>()
        .WithSummary("Checks access to guild")
        .WithDescription("Checks if bot has access to the guild")
        .WithRequestValidation<GuildIdRequest>();

    internal static Results<Ok<Response>, BadRequest<string>> Handle([AsParameters] GuildIdRequest request, [FromServices] IDiscordService discordService)
    {
        var (success, name, message) = discordService.CheckGuildId(request.GuildId);
        return success ? TypedResults.Ok(new Response(success, name, message)) : TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record Response(bool Valid, string Name, string Message);
}