using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class DeleteTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{guildId}", Handle)
        .SetOpenApiOperationId<DeleteTown>()
        .WithSummary("Deletes the town in the provided server")
        .WithDescription("Removes all roles, channels and categories associated with clocktower")
        .WithRequestValidation<GuildIdRequest>();

    internal static async Task<Results<Ok<string>,  BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, [FromServices] IDiscordTownService discordTownService)
    {
        var (success, message) = await discordTownService.DeleteTown(request.GuildId);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
    }
}