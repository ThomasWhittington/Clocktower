namespace Clocktower.Server.DiscordTown.Endpoints.Validation;

[UsedImplicitly]
public record GuildIdRequest(string GuildId);

[UsedImplicitly]
public class GuildIdRequestValidator : AbstractValidator<GuildIdRequest>
{
    public GuildIdRequestValidator()
    {
        RuleFor(x => x.GuildId)
            .NotEmpty()
            .WithMessage("GuildId cannot be empty")
            .Must(Common.Validation.BeValidDiscordSnowflake)
            .WithMessage("GuildId must be a valid Discord snowflake");
    }
}