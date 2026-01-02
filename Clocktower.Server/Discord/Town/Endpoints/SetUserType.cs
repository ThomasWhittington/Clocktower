using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class SetUserType : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/{userId}/set-type/{userType}", Handle)
        .SetOpenApiOperationId<SetUserType>()
        .RequireAuthorization("StoryTellerForGame")
        .WithSummaryAndDescription("Sets the userType for a user")
        .WithRequestValidation<Request>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IDiscordTownService discordTownService)
    {
        var result = await discordTownService.SetUserType(request.GameId, request.UserId, request.UserType);
        return result.ToHttpResult();
    }

    [UsedImplicitly]
    public record Request(string GameId, string UserId, UserType UserType);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(Request.UserId));
            RuleFor(x => x.UserType).NotNull().NotEqual(UserType.Unknown);
        }
    }
}