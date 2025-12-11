namespace Clocktower.Server.Data;

public record TownUser(string Id, string Name, string AvatarUrl) : ITownUser
{
    public VoiceState VoiceState { get; set; } = null!;
    public bool IsPresent { get; set; }
}

public interface ITownUser : IIdentifiable
{
    [UsedImplicitly] string Name { get; }
    [UsedImplicitly] string AvatarUrl { get; }
    [UsedImplicitly] VoiceState VoiceState { get; set; }
    [UsedImplicitly] bool IsPresent { get; set; }
}