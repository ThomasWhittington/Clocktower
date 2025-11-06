using Clocktower.Server.Discord.Town.Endpoints.Validation;
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

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);
        
        var (success, message) = await discordTownService.CreateTown(guildId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }
}