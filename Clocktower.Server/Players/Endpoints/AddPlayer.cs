namespace Clocktower.Server.Players.Endpoints;

[UsedImplicitly]
public class AddPlayer : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/", Handle)
        .WithSummary("Adds player to the circle")
        .WithRequestValidation<Request>();

    public static Ok<Response> Handle(Request request, CancellationToken cancellationToken)
    {
        var response = new Response(request.Name.Trim());
        return TypedResults.Ok(response);
    }

    [UsedImplicitly]
    public record Request(string Name);

    public record Response(string Name);

    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name.Trim())
                .MinimumLength(2)
                .WithMessage("Player name cannot be less than 2 characters")
                .MaximumLength(10)
                .WithMessage("Player name cannot be longer than 10 characters");
        }
    }
}