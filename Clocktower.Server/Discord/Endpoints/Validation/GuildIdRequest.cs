namespace Clocktower.Server.Discord.Endpoints.Validation;

[UsedImplicitly]
public record GuildIdRequest(ulong GuildId);

[UsedImplicitly]
public class GuildIdRequestValidator : AbstractValidator<GuildIdRequest>
{
    public GuildIdRequestValidator()
    {
        RuleFor(x => x.GuildId)
            .NotEmpty()
            .WithMessage("GuildId cannot be empty")
            .Must(BeValidDiscordSnowflake)
            .WithMessage("GuildId must be a valid Discord snowflake");
    }

    private static bool BeValidDiscordSnowflake(ulong id)
    {
        return id > 41943040000L;
    }
}