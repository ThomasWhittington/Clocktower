using System.Reflection;

namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    private static readonly Lazy<IReadOnlyList<Role>> AllRolesLazy = new(DiscoverAllRoles);

    public static IReadOnlyList<Role> AllRoles => AllRolesLazy.Value;

    private static IReadOnlyList<Role> DiscoverAllRoles()
    {
        var roles = new List<Role>();
        var roleType = typeof(Role);

        var roleMethods = roleType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.ReturnType == roleType &&
                        m.GetParameters().Length == 0 &&
                        !m.Name.Equals("get_" + nameof(AllRoles)) &&
                        !m.IsSpecialName)
            .ToList();

        foreach (var method in roleMethods)
        {
            if (method.Invoke(null, null) is Role role)
            {
                roles.Add(role);
            }
        }

        return roles.Distinct().ToList().AsReadOnly();
    }
}