namespace Clocktower.Server.Discord.Town.Endpoints.Validation;

[UsedImplicitly]
public record GameAndUserRequest(string GameId, string UserId);

[UsedImplicitly]
public class GameAndUserRequestValidator : AbstractValidator<GameAndUserRequest>
{
    public GameAndUserRequestValidator()
    {
        RuleFor(x => x.GameId).MustBeValidGameId();
        RuleFor(x => x.UserId).MustBeValidSnowflake(nameof(GameAndUserRequest.UserId));
    }
}