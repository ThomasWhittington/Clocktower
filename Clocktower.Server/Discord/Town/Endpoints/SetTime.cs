using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

[UsedImplicitly]
public class SetTime : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/time", Handle)
        .SetOpenApiOperationId<SetTime>()
        .WithSummary("Sets the time of the town")
        .WithDescription("Sets the game state of the town based on the day time");


    private static async Task<Results<Ok<string>, BadRequest<string>>> Handle([AsParameters] Request request, IDiscordTownService discordTownService)
    {
        var guildId = ulong.Parse(request.GuildId);

        var (success, message) = await discordTownService.SetTime(guildId, request.GameTime);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(string GuildId, GameTime GameTime);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GuildId)
                .NotEmpty()
                .WithMessage("GuildId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("GuildId must be a valid Discord snowflake");

            RuleFor(x => x.GameTime)
                .NotEmpty()
                .WithMessage("GameTime cannot be empty");
        }
    }
}