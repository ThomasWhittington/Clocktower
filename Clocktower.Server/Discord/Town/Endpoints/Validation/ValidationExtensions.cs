namespace Clocktower.Server.Discord.Town.Endpoints.Validation;

public static class ValidationExtensions
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> MustBeValidSnowflake(string fieldName)
        {
            return ruleBuilder
                .NotNull().WithMessage($"{fieldName} cannot be null")
                .NotEmpty().WithMessage($"{fieldName} cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage($"{fieldName} must be a valid Discord snowflake");
        }

        public IRuleBuilderOptions<T, string> MustBeValidGameId()
        {
            return ruleBuilder
                .MinimumLength(3).WithMessage("GameId cannot be less than 3 characters")
                .MaximumLength(32).WithMessage("GameId cannot be longer than 32 characters");
        }
    }
}