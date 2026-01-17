namespace Clocktower.Server.Data.Stores;

public interface IGamePerspectiveStore
{
    const string OmniscientKey = "omniscient";


    IEnumerable<GamePerspective> GetAll();
    GamePerspective? Get(string gameId, string userId);
    bool Add(string gameId, string perspectiveKey, GamePerspective perspective);
    bool Remove(string gameId, string perspectiveKey);
    void TryUpdate(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction);
    IEnumerable<GamePerspective> GetAllPerspectivesForGame(string gameId);
    void UpdateAllPerspectives(string gameId, Func<GamePerspective, GamePerspective> updateFunction);
    void UpdateUserInOwnAndOmniscientPerspectives(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction);
    void UpdateUserInOwnPerspective(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction);
}