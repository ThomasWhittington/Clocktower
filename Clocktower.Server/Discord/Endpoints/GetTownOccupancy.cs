using Clocktower.Server.Discord.Endpoints.Validation;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class GetTownOccupancy : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/occupancy", Handle)
        .WithSummary("Get occupancy of town")
        .WithDescription("Gets user presense in the town")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<TownOccupants>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var (success, townOccupants, message) = await discordService.GetTownOccupancy(request.GuildId);
        if (success)
        {
            return TypedResults.Ok(townOccupants);
        }

        return TypedResults.BadRequest(message);
    }
}