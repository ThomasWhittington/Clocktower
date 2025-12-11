using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class GetDiscordTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{gameId}/occupancy", Handle)
        .SetOpenApiOperationId<GetDiscordTown>()
        .WithSummary("Get occupancy of town")
        .WithDescription("Gets user presence in the town")
        .WithRequestValidation<GameIdRequest>();

    internal static async Task<Results<Ok<DiscordTownDto>, BadRequest<string>>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IDiscordTownService discordTownService)
    {
        var (success, discordTownDto, message) = await discordTownService.GetDiscordTownDto(request.GameId);
        return success && discordTownDto is not null ? TypedResults.Ok(discordTownDto) : TypedResults.BadRequest(message);
    }
}