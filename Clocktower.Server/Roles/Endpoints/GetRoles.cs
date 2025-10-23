namespace Clocktower.Server.Roles.Endpoints;

[UsedImplicitly]
public class GetRoles : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .WithSummary("Return all roles")
        .WithDescription("Filter roles by edition and/or role type");

    [UsedImplicitly]
    public record Response(IEnumerable<Role> Roles);

    [UsedImplicitly]
    public record Request(RoleType? RoleType = null, Edition? Edition = null);


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
}