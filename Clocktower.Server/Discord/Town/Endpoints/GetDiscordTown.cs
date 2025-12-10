using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class GetDiscordTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/occupancy", Handle)
        .SetOpenApiOperationId<GetDiscordTown>()
        .WithSummary("Get occupancy of town")
        .WithDescription("Gets user presense in the town")
        .WithRequestValidation<GuildIdRequest>();

    internal static async Task<Results<Ok<DiscordTown>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, [FromServices]IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, discordTown, message) = await discordTownService.GetDiscordTown(guildId);
        return success ? TypedResults.Ok(discordTown) : TypedResults.BadRequest(message);
    }
}