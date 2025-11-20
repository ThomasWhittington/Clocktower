using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class SetTime : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/time", Handle)
        .SetOpenApiOperationId<SetTime>()
        .WithSummary("Sets the time of the town")
        .WithDescription("Sets the game state of the town based on the day time");


    private static async Task<Results<Ok<string>, BadRequest<string>>> Handle([AsParameters] Request request, IDiscordTownService discordTownService)
    {
        var gameId = request.GameId.Trim();

        var (success, message) = await discordTownService.SetTime(gameId, request.GameTime);
        return success ? TypedResults.Ok(message) : TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(string GameId, GameTime GameTime);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId)
                .NotEmpty()
                .WithMessage("GameId cannot be empty");

            RuleFor(x => x.GameTime)
                .NotEmpty()
                .WithMessage("GameTime cannot be empty");
        }
    }
}