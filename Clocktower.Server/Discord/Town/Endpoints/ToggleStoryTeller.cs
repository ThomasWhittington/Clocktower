using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

//TODO change toggleStoryTeller to be SetUserType
[UsedImplicitly]
public class ToggleStoryTeller : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/{userId}", Handle)
        .SetOpenApiOperationId<ToggleStoryTeller>()
        .WithSummary("Toggles the storyteller role for a user")
        .WithDescription("Adds or removes the storyteller role for the specified user")
        .WithRequestValidation<GameAndUserRequest>();

    internal static async Task<Results<Ok<string>, BadRequest<string>>> Handle(
        [AsParameters] GameAndUserRequest request,
        [FromServices] IDiscordTownService discordTownService)
    {
        var (success, message) = await discordTownService.ToggleStoryTeller(request.GameId.Trim(), request.UserId);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
    }
}