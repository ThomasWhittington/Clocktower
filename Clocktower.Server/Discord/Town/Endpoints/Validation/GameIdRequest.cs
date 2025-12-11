namespace Clocktower.Server.Discord.Town.Endpoints.Validation;

[UsedImplicitly]
public record GameIdRequest(string GameId);

[UsedImplicitly]
public class GameIdRequestValidator : AbstractValidator<GameIdRequest>
{
    public GameIdRequestValidator()
    {
        RuleFor(x => x.GameId).MustBeValidGameId();
    }
}