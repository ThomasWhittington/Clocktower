namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetDraftBluff : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/{userId}/set-draft-bluff/{slot}/{roleId?}", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<SetDraftBluff>()
        .WithSummaryAndDescription("Sets a draft bluff for a player in a game")
        .WithRequestValidation<Request>();

    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.UpdateDraftBluff(request.GameId, request.UserId, request.Slot, request.RoleId);
        return result.ToHttpResult();
    }

    [UsedImplicitly]
    public record Request(string GameId, string UserId, int Slot, string? RoleId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(Request.UserId));
            RuleFor(x => x.Slot).InclusiveBetween(1, 3);
        }
    }
}