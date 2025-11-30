using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class CreateTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}", Handle)
        .SetOpenApiOperationId<CreateTown>()
        .WithSummary("Creates the town in the provided server")
        .WithDescription("Creates the roles, categories and channels required for clocktower")
        .WithRequestValidation<GuildIdRequest>();

    internal static async Task<Results<Ok<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, [FromServices] IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, message) = await discordTownService.CreateTown(guildId);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
    }
}