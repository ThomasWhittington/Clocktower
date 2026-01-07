namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetPlayerIsDead : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/set-player-is-dead/{userId}/{isDead:bool}", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<SetPlayerIsDead>()
            .WithSummaryAndDescription("Sets a player's dead status in the game")
            .WithRequestValidation<Request>();
    }

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = await gamePerspectiveService.SetPlayerIsDead(request.GameId, request.UserId, request.IsDead);
        return result.ToHttpResult();
    }


    [UsedImplicitly]
    public record Request(string GameId, string UserId, bool IsDead);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(Request.UserId));
            RuleFor(x => x.IsDead).NotNull();
        }
    }
}