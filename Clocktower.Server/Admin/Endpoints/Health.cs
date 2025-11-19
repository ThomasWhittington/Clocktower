namespace Clocktower.Server.Admin.Endpoints;

[UsedImplicitly]
public class Health : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/health", Handle)
        .SetOpenApiOperationId<Health>()
        .WithSummary("Checks the health of the server")
        .WithDescription("Checks the health of the server");

    internal static Ok<Response> Handle()
    {
        return TypedResults.Ok(new Response("Healthy", DateTime.UtcNow));
    }

    [UsedImplicitly]
    public record Response(string Status, DateTime TimeStamp);
}