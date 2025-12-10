namespace Clocktower.Server.Data;

public record GameUser(string Id) : IIdentifiable
{
    public bool IsPlaying { get; set; }
    public UserType UserType { get; set; } = UserType.Unknown;
}