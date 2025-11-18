namespace Clocktower.Server.Data;

public class GameUser(string id, string name, string avatarUrl)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string AvatarUrl { get; set; } = avatarUrl;
    public bool IsPlaying { get; set; }
    public bool IsPresent { get; set; }
    public UserType UserType { get; set; } = UserType.Unknown;
}