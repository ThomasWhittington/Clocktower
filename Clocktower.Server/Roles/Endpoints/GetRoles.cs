namespace Clocktower.Server.Roles.Endpoints;

[UsedImplicitly]
public class GetRoles : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .SetOpenApiOperationId<GetRoles>()
        .WithSummary("Return all roles")
        .WithDescription("Filter roles by edition and/or role type");


    public static Response Handle([AsParameters] Request request)
    {
        var roles = Role.AllRoles.AsEnumerable();
        if (request.Edition.HasValue)
        {
            roles = roles.Where(o => o.Edition == request.Edition);
        }

        if (request.RoleType.HasValue)
        {
            roles = roles.Where(o => o.Type == request.RoleType);
        }

        return new Response(roles);
    }

    [UsedImplicitly]
    public record Response(IEnumerable<Role> Roles);

    [UsedImplicitly]
    public record Request(RoleType? RoleType = null, Edition? Edition = null);
}