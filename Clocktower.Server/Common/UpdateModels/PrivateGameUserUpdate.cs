namespace Clocktower.Server.Common.UpdateModels;

public record PrivateGameUserUpdate
{
    public Role? Role { get; init; }
    public bool RemoveRole { get; init; }
}