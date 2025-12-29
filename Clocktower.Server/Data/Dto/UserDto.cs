namespace Clocktower.Server.Data.Dto;

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

    public static UserDto FromGameUser(GameUser gameUser, TownUser? townUser = null)
    {
        return new UserDto(gameUser.Id, townUser?.Name ?? gameUser.Id, townUser?.AvatarUrl ?? string.Empty)
        {
            VoiceState = townUser?.VoiceState ?? new VoiceState(false, false, false, false),
            IsPresent = townUser?.IsPresent ?? false,
            IsPlaying = gameUser.IsPlaying,
            UserType = gameUser.UserType
        };
    }
}