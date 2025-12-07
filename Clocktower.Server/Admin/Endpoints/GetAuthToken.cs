using Clocktower.Server.Admin.Services;

namespace Clocktower.Server.Admin.Endpoints;

[UsedImplicitly]
public class GetAuthToken : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/auth/token", Handle)
        .AllowAnonymous()
        .SetOpenApiOperationId<GetAuthToken>()
        .WithSummary("Get JWT token for testing")
        .WithDescription("Returns a JWT token for API testing purposes");

    internal static Results<ContentHttpResult, BadRequest<string>> Handle(TokenRequest request, [FromServices] IAdminService adminService)
    {
        if (string.IsNullOrEmpty(request.Username))
        {
            return TypedResults.BadRequest("Username is required");
        }

        var (success, result) = adminService.GenerateJwtToken(request.Username);

        return success
            ? TypedResults.Text(result, "text/plain")
            : TypedResults.BadRequest(result);
    }

    [UsedImplicitly]
    public record TokenRequest(string Username);
}