namespace Clocktower.Server.Data;

public class GameState : IIdentifiable
{
    public string Id { get; set; }
    public string GuildId { get; set; }
    public List<GameUser> Users { get; set; } = [];
    public IEnumerable<GameUser> Players => Users.Where(o => o.UserType == UserType.Player);
    public IEnumerable<GameUser> StoryTellers => Users.Where(o => o.UserType == UserType.StoryTeller);
    public IEnumerable<GameUser> Spectators => Users.Where(o => o.UserType == UserType.Spectator);
    public int MaxPlayers { get; set; } = 0;

    public GameUser CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }

    public GameTime GameTime { get; set; }

    public bool IsFull => MaxPlayers != 0 && Players.Count() >= MaxPlayers;
}