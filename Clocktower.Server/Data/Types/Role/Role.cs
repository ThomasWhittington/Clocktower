namespace Clocktower.Server.Data.Types.Role;

public partial record Role(string Name, string Description, RoleType Type, Edition Edition, RoleMetadata Metadata)
{
    public string Id => new string(Name.Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c)).ToArray()).ToLower();
}