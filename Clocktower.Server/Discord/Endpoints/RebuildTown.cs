using Clocktower.Server.Discord.Endpoints.Validation;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class RebuildTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/rebuild", Handle)
        .WithSummary("Rebuild town")
        .WithDescription("Rebuilds the town including roles, categories and channels")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var (success, message) = await discordService.RebuildTown(request.GuildId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }
}