using Clocktower.Server.Discord.Endpoints.Validation;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class CheckGuild : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}", Handle)
        .SetOpenApiOperationId<CheckGuild>()
        .WithSummary("Checks access to guild")
        .WithDescription("Checks if bot has access to the guild")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<Response>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var guildId = ulong.Parse(request.GuildId);
        
        var (success, valid, name, message) = await discordService.CheckGuildId(guildId);
        if (success)
        {
            return TypedResults.Ok(new Response(valid, name, message));
        }

        return TypedResults.BadRequest(message);
    }

    public record Response(bool Valid, string Name, string Message);
}