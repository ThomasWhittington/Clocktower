using Clocktower.Server.Roles.Services;

namespace Clocktower.Server.Roles.Endpoints;

[UsedImplicitly]
public class GetRoles : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetRoles>()
        .WithSummary("Return all roles")
        .WithDescription("Filter roles by edition and/or role type");


    public static Ok<Response> Handle([AsParameters] Request request, [FromServices] IRolesService rolesService)
    {
        var roles = rolesService.GetRoles(request.Edition, request.RoleType);
        return TypedResults.Ok(new Response(roles));
    }

    [UsedImplicitly]
    public record Response(IEnumerable<Role> Roles);

    [UsedImplicitly]
    public record Request(RoleType? RoleType = null, Edition? Edition = null);
}