namespace Clocktower.Server.Data;

public record TownUser(string Id, string Name, string AvatarUrl) : IIdentifiable
{
    public VoiceState VoiceState { get; set; } = null!;
    public bool IsPresent { get; set; }
}