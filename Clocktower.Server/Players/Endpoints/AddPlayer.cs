using Clocktower.Server.Common.Api.Extensions;

namespace Clocktower.Server.Players.Endpoints;

[UsedImplicitly]
public class AddPlayer:IEndpoint
{

    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/", Handle)
        .WithSummary("Adds player to the circle")
        .WithRequestValidation<Request>();

    [UsedImplicitly]
    public record Request(string Name);
    public record Response(string Name);
    
    [UsedImplicitly]
    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(10);
        }
    }
    public static async Task<Ok<Response>> Handle(Request request, CancellationToken cancellationToken)
    {
        var response = new Response(request.Name);
        return TypedResults.Ok(response);
    }
}