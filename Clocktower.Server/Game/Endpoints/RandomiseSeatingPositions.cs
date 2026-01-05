namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class RandomiseSeatingPositions : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/randomise-seating-positions", Handle)
            .RequireAuthorization("StoryTellerForGame")
            .SetOpenApiOperationId<RandomiseSeatingPositions>()
            .WithSummaryAndDescription("Randomises seating positions for players in the game")
            .WithRequestValidation<GameIdRequest>();
    }

    internal static async Task<Results<Ok<string[]>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IGamePerspectiveService gamePerspectiveService)
    {
        var result = await gamePerspectiveService.RandomiseSeatingPositions(request.GameId);
        return result.ToHttpResult();
    }
}