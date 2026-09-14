using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class MoveUserToChannel : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/{channelId}", Handle)
        .SetOpenApiOperationId<MoveUserToChannel>()
        .WithSummaryAndDescription("Moves the user to the specified channel")
        .WithRequestValidation<Request>()
        .RequireAuthorization();

    internal static async Task<Results<Ok<string>, BadRequest<string>>> Handle(ClaimsPrincipal user, [AsParameters] Request request, IDiscordTownService discordTownService)
    {
        var (success, message) = await discordTownService.MoveUser(request.GuildId, user.GetUserId()!, request.ChannelId);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(string GuildId, string ChannelId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GuildId).MustBeValidSnowflake(nameof(Request.GuildId));
            RuleFor(x => x.ChannelId).MustBeValidSnowflake(nameof(Request.ChannelId));
        }
    }
}
