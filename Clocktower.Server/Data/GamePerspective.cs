namespace Clocktower.Server.Data;

public record GamePerspective(string Id, string UserId, string GuildId, GameUser CreatedBy, DateTime CreatedDate) : IIdentifiable
{
    public IReadOnlyList<GameUser> Users { get; init; } = [];
    public IEnumerable<GameUser> Players => Users.Where(o => o.UserType == UserType.Player);
    public IEnumerable<GameUser> StoryTellers => Users.Where(o => o.UserType == UserType.StoryTeller);
    public IEnumerable<GameUser> Spectators => Users.Where(o => o.UserType == UserType.Spectator);
    public GameTime GameTime { get; init; }
}