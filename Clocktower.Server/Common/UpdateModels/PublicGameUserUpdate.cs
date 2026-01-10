namespace Clocktower.Server.Common.UpdateModels;

public record PublicGameUserUpdate
{
    public UserType? UserType { get; init; }
    public bool? IsPlaying { get; init; }
    public int? SeatingPosition { get; init; }
    public bool? HasVoteToken { get; init; }
    public bool? IsDead { get; init; }
    public bool? IsMarked { get; init; }
}