namespace Clocktower.Server.Data;

[UsedImplicitly]
public record UserDto(string Id, string Name, string AvatarUrl) : IGameUser, ITownUser
{
    [UsedImplicitly] public VoiceState VoiceState { get; set; } = new(false, false, false, false);
    [UsedImplicitly] public bool IsPresent { get; set; }
    [UsedImplicitly] public bool IsPlaying { get; set; }
    [UsedImplicitly] public UserType UserType { get; set; } = UserType.Unknown;

    public static UserDto FromTownUser(TownUser townUser, GameUser? gameUser = null)
    {
        return new UserDto(townUser.Id, townUser.Name, townUser.AvatarUrl)
        {
            VoiceState = townUser.VoiceState,
            IsPresent = townUser.IsPresent,
            IsPlaying = gameUser?.IsPlaying ?? false,
            UserType = gameUser?.UserType ?? UserType.Unknown
        };
    }
}