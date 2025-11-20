using Clocktower.Server.Discord.Services;
using Clocktower.Server.Discord.Town.Endpoints.Validation;

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

    internal static Results<Ok<Response>, BadRequest<string>> Handle([AsParameters] GuildIdRequest request,[FromServices] IDiscordService discordService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, valid, name, message) = discordService.CheckGuildId(guildId);
        return success ? TypedResults.Ok(new Response(valid, name, message)) : TypedResults.BadRequest(message);
    }

    [UsedImplicitly]
    public record Response(bool Valid, string Name, string Message);
}