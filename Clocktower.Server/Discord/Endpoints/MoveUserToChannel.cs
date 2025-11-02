using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class MoveUserToChannel : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/{guildId}/{userId}/{channelId}", Handle)
        .SetOpenApiOperationId<MoveUserToChannel>()
        .WithSummary("Moves the user to the specified channel")
        .WithDescription("Moves the user to the specified channel")
        .WithRequestValidation<Request>();

    private static async Task<Results<Ok<string>, BadRequest<string>>> Handle([AsParameters] Request request, DiscordService discordService)
    {
        var (success, message) = await discordService.MoveUser(request.GuildId, request.UserId, request.ChannelId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(ulong GuildId, ulong UserId, ulong ChannelId);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.GuildId)
                .NotEmpty()
                .WithMessage("GuildId cannot be empty")
                .Must(BeValidDiscordSnowflake)
                .WithMessage("GuildId must be a valid Discord snowflake");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");

            RuleFor(x => x.ChannelId)
                .NotEmpty()
                .WithMessage("ChannelId cannot be empty")
                .Must(BeValidDiscordSnowflake)
                .WithMessage("ChannelId must be a valid Discord snowflake");
        }

        private static bool BeValidDiscordSnowflake(ulong id)
        {
            return id > 41943040000L;
        }
    }
}