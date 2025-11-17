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
        .WithRequestValidation<Request>();

    private static async Task<Ok> Handle([AsParameters] Request request, IDiscordTownService discordTownService)
    {
        await discordTownService.PingUser(request.UserId);
        return TypedResults.Ok();
    }

    [UsedImplicitly]
    public record Request(string UserId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");
        }
    }
}