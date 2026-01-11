namespace Clocktower.Server.Data.Types.Role;

public partial record Role(string Name, string Description, RoleType Type, Edition Edition)
{
    [UsedImplicitly] public string Id => new string(Name.Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c)).ToArray()).ToLower();
    public RoleMetadata Metadata { get; set; } = new();
}