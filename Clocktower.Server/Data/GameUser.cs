namespace Clocktower.Server.Data;

public record GameUser(string Id) : IGameUser
{
    public bool IsPlaying { get; set; }
    public UserType UserType { get; set; } = UserType.Unknown;
    public int SeatingPosition { get; set; }
}

public interface IGameUser : IIdentifiable
{
    [UsedImplicitly] bool IsPlaying { get; set; }
    [UsedImplicitly] UserType UserType { get; set; }
    [UsedImplicitly] int SeatingPosition { get; set; }
}