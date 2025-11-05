using Clocktower.Server.Discord.Town.Services;

namespace Clocktower.Server.Discord.Town.Endpoints;

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
        var guildId = ulong.Parse(request.GuildId);
        var userId = ulong.Parse(request.UserId);
        var channelId = ulong.Parse(request.ChannelId);

        var (success, message) = await discordService.MoveUser(guildId, userId, channelId);
        if (success)
        {
            return TypedResults.Ok(message);
        }

        return TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(string GuildId, string UserId, string ChannelId);

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

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");

            RuleFor(x => x.ChannelId)
                .NotEmpty()
                .WithMessage("ChannelId cannot be empty")
                .Must(Common.Validation.BeValidDiscordSnowflake)
                .WithMessage("ChannelId must be a valid Discord snowflake");
        }
    }
}