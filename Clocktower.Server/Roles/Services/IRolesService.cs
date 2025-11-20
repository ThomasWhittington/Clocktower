namespace Clocktower.Server.Roles.Services;

public interface IRolesService
{
    IEnumerable<Role> GetRoles(Edition? edition, RoleType? roleType, IEnumerable<Role>? roles = null);
}