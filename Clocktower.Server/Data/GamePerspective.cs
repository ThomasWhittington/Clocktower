namespace Clocktower.Server.Data;

public record GamePerspective(string Id, string UserId, string GuildId, GameUser CreatedBy, DateTime CreatedDate) : IIdentifiable
{
    public IReadOnlyList<GameUser> Users { get; init; } = [];
    public IEnumerable<GameUser> Players => Users.Where(o => o.UserType == UserType.Player);
    public IEnumerable<GameUser> StoryTellers => Users.Where(o => o.UserType == UserType.StoryTeller);
    public IEnumerable<GameUser> Spectators => Users.Where(o => o.UserType == UserType.Spectator);
    public GameTime GameTime { get; init; }
    public Script? Script { get; init; }

    public RoleDistribution? DefaultRoleDistributions
    {
        get
        {
            int playerCount = Players.Count();
            if (playerCount < 5) return null;

            const int demons = 1;
            int minions = (playerCount - 7) / 3 + 1;
            int outsiders = playerCount switch
            {
                5 => 0,
                6 => 1,
                _ => (playerCount - 7) % 3
            };
            int townsfolk = playerCount - outsiders - minions - demons;

            return new RoleDistribution(townsfolk, outsiders, minions, demons);
        }
    }
}

public record RoleDistribution(int Townsfolk, int Outsiders, int Minions, int Demons);