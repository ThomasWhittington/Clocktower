namespace Clocktower.Server.Roles.Services;

public class RolesService : IRolesService
{
    public IEnumerable<Role> GetRoles(Edition? edition, RoleType? roleType, IEnumerable<Role>? roles = null)
    {
        roles ??= Role.AllRoles.AsEnumerable();
        
        if (edition.HasValue)
        {
            roles = roles.Where(o => o.Edition == edition);
        }

        if (roleType.HasValue)
        {
            roles = roles.Where(o => o.Type == roleType);
        }

        return roles;
    }
}