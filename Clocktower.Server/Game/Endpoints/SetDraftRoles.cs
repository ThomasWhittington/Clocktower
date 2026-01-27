namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetDraftRoles : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/set-draft-roles", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<SetDraftRoles>()
        .WithSummaryAndDescription("Sets draft roles for multiple players")
        .WithRequestValidation<Request>();


    internal static async Task<Results<Ok<string>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle(
        [AsParameters] Request request,
        [FromServices] IGameService gameService)
    {
        var result = await gameService.SetDraftRoles(request.GameId, request.Body.PlayerRoles);
        return result.ToHttpResult();
    }


    [UsedImplicitly]
    public record Body(Dictionary<string, string> PlayerRoles);

    [UsedImplicitly]
    public record Request(string GameId, Body Body);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.Body.PlayerRoles).NotEmpty();
        }
    }
}