namespace Clocktower.Server.Data.Types.Role;

public partial record Role
{
    public static Role Townsfolk(string name, string description, Edition edition, RoleMetadata metadata) => new(name, description, RoleType.Townsfolk, edition, metadata);
    public static Role Outsider(string name, string description, Edition edition, RoleMetadata metadata) => new(name, description, RoleType.Outsider, edition, metadata);
    public static Role Minion(string name, string description, Edition edition, RoleMetadata metadata) => new(name, description, RoleType.Minion, edition, metadata);
    public static Role Demon(string name, string description, Edition edition, RoleMetadata metadata) => new(name, description, RoleType.Demon, edition, metadata);
    public static Role Traveller(string name, string description, Edition edition, RoleMetadata metadata) => new(name, description, RoleType.Traveller, edition, metadata);
}