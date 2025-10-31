using Clocktower.Server.Discord.Endpoints.Validation;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class GetTownStatus : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/status", Handle)
        .SetOpenApiOperationId<GetTownStatus>()
        .WithSummary("Get status of town")
        .WithDescription("Gets if the town exists in a valid state")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<TownExistsResponse>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var (success, exists, message) = await discordService.TownExists(request.GuildId);
        if (success)
        {
            return TypedResults.Ok(new TownExistsResponse(exists, message));
        }

        return TypedResults.BadRequest(message);
    }

    public record TownExistsResponse(bool Exists, string Message);
}