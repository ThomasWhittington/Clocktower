namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    public static Role Townsfolk(string name, string description, Edition edition) => new(name, description, RoleType.Townsfolk, edition);
    public static Role Outsider(string name, string description, Edition edition) => new(name, description, RoleType.Outsider, edition);
    public static Role Minion(string name, string description, Edition edition) => new(name, description, RoleType.Minion, edition);
    public static Role Demon(string name, string description, Edition edition) => new(name, description, RoleType.Demon, edition);
    public static Role Traveller(string name, string description, Edition edition) => new(name, description, RoleType.Traveller, edition);
}