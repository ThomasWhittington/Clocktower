namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SwapSeatingPositions : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/swap-seating-positions/{userId1}/{userId2}", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<SwapSeatingPositions>()
            .WithSummaryAndDescription("Swaps the seats for two players in the game")
            .WithRequestValidation<Request>();
    }

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = await gamePerspectiveService.SwapSeatingPositions(request.GameId, request.UserId1, request.UserId2);
        return result.ToHttpResult();
    }


    [UsedImplicitly]
    public record Request(string GameId, string UserId1, string UserId2);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.UserId1).MustBeValidSnowflake(nameof(Request.UserId1));
            RuleFor(x => x.UserId2).MustBeValidSnowflake(nameof(Request.UserId2));
        }
    }
}