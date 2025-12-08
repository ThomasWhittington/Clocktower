namespace Clocktower.Server.Data;

public record GameUser(string Id, string Name, string AvatarUrl) : IIdentifiable
{
    public bool IsPlaying { get; set; }
    public bool IsPresent { get; set; }
    public UserType UserType { get; set; } = UserType.Unknown;
    public VoiceState VoiceState { get; set; } = null!;
}