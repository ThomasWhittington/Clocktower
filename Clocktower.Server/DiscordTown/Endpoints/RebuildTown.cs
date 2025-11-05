using Clocktower.Server.DiscordTown.Endpoints.Validation;
using Clocktower.Server.DiscordTown.Services;

namespace Clocktower.Server.DiscordTown.Endpoints;

[UsedImplicitly]
public class RebuildTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/rebuild", Handle)
        .SetOpenApiOperationId<RebuildTown>()
        .WithSummary("Rebuild town")
        .WithDescription("Rebuilds the town including roles, categories and channels")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, message) = await discordService.RebuildTown(guildId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }
}