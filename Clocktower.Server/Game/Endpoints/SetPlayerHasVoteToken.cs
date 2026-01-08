namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetPlayerHasVoteToken : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/set-player-has-vote-token/{userId}/{hasVoteToken:bool}", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<SetPlayerHasVoteToken>()
            .WithSummaryAndDescription("Sets if a player has a vote token in the game")
            .WithRequestValidation<Request>();
    }

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = await gamePerspectiveService.SetPlayerHasVoteToken(request.GameId, request.UserId, request.HasVoteToken);
        return result.ToHttpResult();
    }


    [UsedImplicitly]
    public record Request(string GameId, string UserId, bool HasVoteToken);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(Request.UserId));
            RuleFor(x => x.HasVoteToken).NotNull();
        }
    }
}