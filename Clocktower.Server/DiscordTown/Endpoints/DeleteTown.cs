using Clocktower.Server.DiscordTown.Endpoints.Validation;
using Clocktower.Server.DiscordTown.Services;

namespace Clocktower.Server.DiscordTown.Endpoints;

[UsedImplicitly]
public class DeleteTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{guildId}", Handle)
        .SetOpenApiOperationId<DeleteTown>()
        .WithSummary("Deletes the town in the provided server")
        .WithDescription("Removes all roles, channels and categories associated with clocktower")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, message) = await discordService.DeleteTown(guildId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }
}