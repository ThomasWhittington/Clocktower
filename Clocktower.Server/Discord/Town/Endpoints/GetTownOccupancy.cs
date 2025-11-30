using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class GetTownOccupancy : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/occupancy", Handle)
        .SetOpenApiOperationId<GetTownOccupancy>()
        .WithSummary("Get occupancy of town")
        .WithDescription("Gets user presense in the town")
        .WithRequestValidation<GuildIdRequest>();

    internal static async Task<Results<Ok<TownOccupants>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, [FromServices]IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, townOccupants, message) = await discordTownService.GetTownOccupancy(guildId);
        return success ? TypedResults.Ok(townOccupants) : TypedResults.BadRequest(message);
    }
}