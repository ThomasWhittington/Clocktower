using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class PingUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/ping/{userId}", Handle)
        .SetOpenApiOperationId<PingUser>()
        .WithSummary("Pings user")
        .WithDescription("Sends a ping to the user if online")
        .WithRequestValidation<UserIdRequest>();

    internal static async Task<Ok> Handle([AsParameters] UserIdRequest request, [FromServices] IDiscordTownService discordTownService)
    {
        await discordTownService.PingUser(request.UserId);
        return TypedResults.Ok();
    }
}