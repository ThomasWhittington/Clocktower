using System.Reflection;

namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    public const string NoAbilityText = "No Ability";
    private static readonly Lazy<IReadOnlyList<Role>> AllRolesLazy = new(DiscoverAllRoles);

    public static IReadOnlyList<Role> AllRoles => AllRolesLazy.Value;

    private static IReadOnlyList<Role> DiscoverAllRoles()
    {
        var roles = new List<Role>();
        var roleType = typeof(Role);

        var roleProperties = roleType.GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == roleType &&
                        !p.Name.Equals(nameof(AllRoles)))
            .ToList();

        foreach (var property in roleProperties)
        {
            if (property.GetValue(null) is Role role)
            {
                roles.Add(role);
            }
        }

        return roles.Distinct().ToList().AsReadOnly();
    }
}