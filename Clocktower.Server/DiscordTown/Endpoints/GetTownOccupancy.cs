using Clocktower.Server.DiscordTown.Endpoints.Validation;
using Clocktower.Server.DiscordTown.Services;

namespace Clocktower.Server.DiscordTown.Endpoints;

[UsedImplicitly]
public class GetTownOccupancy : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/occupancy", Handle)
        .SetOpenApiOperationId<GetTownOccupancy>()
        .WithSummary("Get occupancy of town")
        .WithDescription("Gets user presense in the town")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<TownOccupants>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, townOccupants, message) = await discordService.GetTownOccupancy(guildId);
        if (success)
        {
            return TypedResults.Ok(townOccupants);
        }

        return TypedResults.BadRequest(message);
    }
}