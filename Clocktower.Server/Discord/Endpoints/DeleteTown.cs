using Clocktower.Server.Discord.Endpoints.Validation;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class DeleteTown : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapDelete("/{guildId}", Handle)
        .WithSummary("Deletes the town in the provided server")
        .WithDescription("Removes all roles, channels and categories associated with clocktower")
        .WithRequestValidation<GuildIdRequest>();

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> Handle([AsParameters] GuildIdRequest request, DiscordService discordService)
    {
        var (success, message) = await discordService.DeleteTown(request.GuildId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }
}