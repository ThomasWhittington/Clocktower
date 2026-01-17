using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public class GamePerspectiveStore : IGamePerspectiveStore
{
    private readonly ConcurrentDictionary<(string, string), GamePerspective> _store = new();

    public IEnumerable<GamePerspective> GetAll() => _store.Values;

    public GamePerspective? Get(string gameId, string userId)
        => _store.TryGetValue((gameId, userId), out var p) ? p : null;

    public bool Add(string gameId, string perspectiveKey, GamePerspective perspective)
        => _store.TryAdd((gameId, perspectiveKey), perspective);

    public bool Remove(string gameId, string perspectiveKey)
        => _store.TryRemove((gameId, perspectiveKey), out _);


    public IEnumerable<GamePerspective> GetAllPerspectivesForGame(string gameId)
        => _store.Where(kvp => kvp.Key.Item1 == gameId).Select(kvp => kvp.Value);


    public void UpdateAllPerspectives(string gameId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        var perspectiveIds = _store.Keys.Where(key => key.Item1 == gameId).Select(key => key.Item2).ToList();
        foreach (var userId in perspectiveIds)
        {
            TryUpdate(gameId, userId, updateFunction);
        }
    }

    public void UpdateUserInOwnAndOmniscientPerspectives(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        TryUpdate(gameId, userId, updateFunction);
        TryUpdate(gameId, IGamePerspectiveStore.OmniscientKey, updateFunction);
    }

    public void UpdateUserInOwnPerspective(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        TryUpdate(gameId, userId, updateFunction);
    }

    public void TryUpdate(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        var key = (gameId, userId);
        while (true)
        {
            if (!_store.TryGetValue(key, out var existing)) return;
            var updated = updateFunction(existing);
            if (_store.TryUpdate(key, updated, existing)) return;
        }
    }
}