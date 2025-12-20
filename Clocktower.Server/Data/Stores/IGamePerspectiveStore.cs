namespace Clocktower.Server.Data.Stores;

public interface IGamePerspectiveStore
{
    bool GameExists(string gameId);
    void Clear();
    GamePerspective? Get(string gameId, string userId);
    bool Set(GamePerspective perspective, string userId);
    bool RemovePerspective(string gameId, string userId);
    bool RemoveGame(string gameId);
    IEnumerable<GamePerspective> GetGuildGames(string guildId);
    IEnumerable<GamePerspective> GetAllPerspectivesForGame(string gameId);
    GamePerspective? GetFirstPerspective(string gameId);
    IEnumerable<GamePerspective> GetUserGames(string userId);
    IEnumerable<GamePerspective> GetAll();
    void AddUserToGame(string gameId, GameUser gameUser);
    void SetTime(string gameId, GameTime gameTime);
    bool UpdateUser(string gameId, string affectedUserId, UserType? userType = null, bool? isPlaying = null);
}