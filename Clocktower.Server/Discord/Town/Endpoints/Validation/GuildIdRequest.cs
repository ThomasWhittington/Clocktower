namespace Clocktower.Server.Discord.Town.Endpoints.Validation;

[UsedImplicitly]
public record GuildIdRequest(string GuildId);

[UsedImplicitly]
public class GuildIdRequestValidator : AbstractValidator<GuildIdRequest>
{
    public GuildIdRequestValidator()
    {
        RuleFor(x => x.GuildId).MustBeValidSnowflake(nameof(GuildIdRequest.GuildId));
    }
}