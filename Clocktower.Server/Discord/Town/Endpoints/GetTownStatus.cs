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

    internal static Results<Ok<Response>, BadRequest<string>> Handle([AsParameters] GuildIdRequest request, [FromServices] IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, exists, message) = discordTownService.GetTownStatus(guildId);
        return success ? TypedResults.Ok(new Response(exists, message)) : TypedResults.BadRequest(message);
    }

    public record Response(bool Exists, string Message);
}