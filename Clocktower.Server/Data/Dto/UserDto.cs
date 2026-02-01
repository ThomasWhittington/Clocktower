namespace Clocktower.Server.Data.Dto;

[UsedImplicitly]
public record UserDto(string Id, string Name, string AvatarUrl) : IGameUser, ITownUser
{
    public const int NoSeatingPosition = -1;
    [UsedImplicitly] public VoiceState VoiceState { get; set; } = new(false, false, false, false, false);
    [UsedImplicitly] public bool IsPlaying { get; set; }
    [UsedImplicitly] public UserType UserType { get; set; } = UserType.Unknown;
    [UsedImplicitly] public int SeatingPosition { get; set; }
    [UsedImplicitly] public bool HasVoteToken { get; set; }
    [UsedImplicitly] public bool IsDead { get; set; }
    [UsedImplicitly] public bool IsMarked { get; set; }
    [UsedImplicitly] public bool HandUp { get; set; }
    [UsedImplicitly] public Role? Role { get; set; }
    [UsedImplicitly] public Role? DraftRole { get; set; }

    public static UserDto FromTownUser(TownUser townUser, GameUser? gameUser = null)
    {
        return new UserDto(townUser.Id, townUser.Name, townUser.AvatarUrl)
        {
            VoiceState = townUser.VoiceState,
            IsPlaying = gameUser?.IsPlaying ?? false,
            UserType = gameUser?.UserType ?? UserType.Unknown,
            SeatingPosition = gameUser?.SeatingPosition ?? NoSeatingPosition,
            HasVoteToken = gameUser?.HasVoteToken ?? false,
            IsDead = gameUser?.IsDead ?? false,
            IsMarked = gameUser?.IsMarked ?? false,
            HandUp = gameUser?.HandUp ?? false,
            Role = gameUser?.Role ?? null,
            DraftRole = gameUser?.DraftRole ?? null
        };
    }

    public static UserDto FromGameUser(GameUser gameUser, TownUser? townUser = null)
    {
        return new UserDto(gameUser.Id, townUser?.Name ?? gameUser.Id, townUser?.AvatarUrl ?? string.Empty)
        {
            VoiceState = townUser?.VoiceState ?? new VoiceState(false, false, false, false, false),
            IsPlaying = gameUser.IsPlaying,
            UserType = gameUser.UserType,
            SeatingPosition = gameUser.SeatingPosition,
            HasVoteToken = gameUser.HasVoteToken,
            IsDead = gameUser.IsDead,
            IsMarked = gameUser.IsMarked,
            HandUp = gameUser.HandUp,
            Role = gameUser.Role,
            DraftRole = gameUser.DraftRole
        };
    }
}