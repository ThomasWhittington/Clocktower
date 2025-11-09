using Clocktower.Server.Discord.Town.Endpoints.Validation;
using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class GetTownStatus : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/{guildId}/status", Handle)
        .SetOpenApiOperationId<GetTownStatus>()
        .WithSummary("Get status of town")
        .WithDescription("Gets if the town exists in a valid state")
        .WithRequestValidation<GuildIdRequest>();

    private static Results<Ok<Response>, NotFound<string>, BadRequest<string>> Handle([AsParameters] GuildIdRequest request, IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, exists, message) = discordTownService.TownExists(guildId);
        if (success)
        {
            return TypedResults.Ok(new Response(exists, message));
        }

        return TypedResults.BadRequest(message);
    }

    public record Response(bool Exists, string Message);
}