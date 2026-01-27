namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class RandomiseSeatingPositions : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/{gameId}/randomise-seating-positions", Handle)
            .RequireStorytellerForGame()
            .SetOpenApiOperationId<RandomiseSeatingPositions>()
            .WithSummaryAndDescription("Randomises seating positions for players in the game")
            .WithRequestValidation<GameIdRequest>();
    }

    internal static async Task<Results<Ok<string[]>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] GameIdRequest request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.RandomiseSeatingPositions(request.GameId);
        return result.ToHttpResult();
    }
}