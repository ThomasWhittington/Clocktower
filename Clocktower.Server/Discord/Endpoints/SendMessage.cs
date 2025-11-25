using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Discord.Endpoints;

[UsedImplicitly]
public class SendMessage : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/message", Handle)
        .SetOpenApiOperationId<SendMessage>()
        .WithSummaryAndDescription("Sends message to the user")
        .WithRequestValidation<Request>();


    internal static async Task<Results<Ok, BadRequest<string>>> Handle([FromBody] Request request, [FromServices] IDiscordService discordService)
    {
        var userId = ulong.Parse(request.UserId);

        var (success, message) = await discordService.SendMessage(userId, request.Message);
        return success ? TypedResults.Ok() : TypedResults.BadRequest(message);
    }


    [UsedImplicitly]
    public record Request(string UserId, string Message);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId cannot be empty")
                .Must(Validation.BeValidDiscordSnowflake)
                .WithMessage("UserId must be a valid Discord snowflake");

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Message is required");
        }
    }
}