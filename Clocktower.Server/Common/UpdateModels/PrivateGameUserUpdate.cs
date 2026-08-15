namespace Clocktower.Server.Common.UpdateModels;

public record PrivateGameUserUpdate
{
    public IReadOnlyList<Role?>? Bluffs { get; init; }
    public Role? Role { get; init; }
    public bool RemoveRole { get; init; }
}