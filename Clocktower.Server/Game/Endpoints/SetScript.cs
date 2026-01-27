using System.Text.Json;

namespace Clocktower.Server.Game.Endpoints;

[UsedImplicitly]
public class SetScript : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{gameId}/script", Handle)
        .RequireStorytellerForGame()
        .SetOpenApiOperationId<SetScript>()
        .WithSummary("Sets the script of the game")
        .WithDescription("Sets the script of the game. Setting to custom requires a custom script file to be uploaded")
        .WithRequestValidation<Request>();


    internal static async Task<Results<Ok<Script>, NotFound<ErrorResponse>, BadRequest<ErrorResponse>>> Handle([AsParameters] Request request, [FromServices] IGameService gameService)
    {
        var result = await gameService.SetScript(request.GameId, request.ScriptSelect, request.Json);
        return result.ToHttpResult();
    }


    [UsedImplicitly]
    public record Request(string GameId, ScriptSelect ScriptSelect, string? Json);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GameId).MustBeValidGameId();
            RuleFor(x => x.ScriptSelect).Must(script => script != ScriptSelect.Unknown).WithMessage("ScriptSelect cannot be Unknown");
            RuleFor(x => x.Json)
                .NotEmpty().WithMessage("Json must be present if ScriptSelect is Custom")
                .Must(IsValidJson).WithMessage("Json must be valid JSON")
                .When(x => x.ScriptSelect == ScriptSelect.Custom);
            RuleFor(x => x.Json)
                .Empty().WithMessage("Json must be empty when ScriptSelect is not Custom")
                .When(x => x.ScriptSelect != ScriptSelect.Custom);
        }

        private static bool IsValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var _ = JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}