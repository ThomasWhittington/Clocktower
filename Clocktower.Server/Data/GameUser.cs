namespace Clocktower.Server.Data;

public record GameUser(string Id) : IGameUser
{
    public bool IsPlaying { get; set; }
    public UserType UserType { get; set; } = UserType.Unknown;
    public int SeatingPosition { get; set; }
    [UsedImplicitly] public bool HasVoteToken { get; set; }
    [UsedImplicitly] public bool IsDead { get; set; }
    [UsedImplicitly] public bool IsMarked { get; set; }
    [UsedImplicitly] public bool HandUp { get; set; }
    [UsedImplicitly] public Role? Role { get; set; }
    [UsedImplicitly] public Role? DraftRole { get; set; }
}

public interface IGameUser : IIdentifiable
{
    [UsedImplicitly] bool IsPlaying { get; set; }
    [UsedImplicitly] UserType UserType { get; set; }
    [UsedImplicitly] int SeatingPosition { get; set; }
    [UsedImplicitly] bool HasVoteToken { get; set; }
    [UsedImplicitly] bool IsDead { get; set; }
    [UsedImplicitly] bool IsMarked { get; set; }
    [UsedImplicitly] bool HandUp { get; set; }
    [UsedImplicitly] Role? Role { get; set; }
    [UsedImplicitly] Role? DraftRole { get; set; }
}