using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Data.Stores;

public class GamePerspectiveStore : IGamePerspectiveStore
{
    private readonly ConcurrentDictionary<(string, string), GamePerspective> _store = new();

    public bool GameExists(string gameId) => _store.Any(o => o.Key.Item1 == gameId);

    public void Clear() => _store.Clear();

    public GamePerspective? Get(string gameId, string userId) =>
        _store.TryGetValue((gameId, userId), out var state) ? state : null;

    public bool Set(GamePerspective perspective) => _store.TryAdd((perspective.Id, perspective.UserId), perspective);

    public bool RemovePerspective(string gameId, string userId) => _store.TryRemove((gameId, userId), out _);

    public bool RemoveGame(string gameId)
    {
        var keysToRemove = _store.Keys.Where(key => key.Item1 == gameId).ToList();
        foreach (var key in keysToRemove)
        {
            _store.TryRemove(key, out _);
        }

        return keysToRemove.Count > 0;
    }

    public IEnumerable<string> GetGuildGameIds(string guildId)
    {
        return _store.Where(g => g.Value.GuildId == guildId)
            .DistinctBy(g => g.Key.Item1)
            .Select(g => g.Key.Item1);
    }

    public IEnumerable<GamePerspective> GetAllPerspectivesForGame(string gameId)
    {
        return _store.Where(kvp => kvp.Key.Item1 == gameId).Select(kvp => kvp.Value);
    }

    public IEnumerable<GamePerspective> GetUserGames(string userId)
    {
        return _store.Where(kvp => kvp.Key.Item2 == userId).Select(kvp => kvp.Value);
    }


    public IEnumerable<GamePerspective> GetAll() => _store.Values;
    public GamePerspective? GetFirstPerspective(string gameId) => _store.FirstOrDefault(kvp => kvp.Key.Item1 == gameId).Value;

    public void AddUserToGame(string gameId, GameUser gameUser)
    {
        var existingPerspective = GetFirstPerspective(gameId);
        if (existingPerspective is null) return;

        var newUserPerspective = existingPerspective with
        {
            UserId = gameUser.Id,
            Users = existingPerspective.Users.Select(ToPublicUser).Append(gameUser).ToList()
        };
        _store.TryAdd((gameId, gameUser.Id), newUserPerspective);

        if (existingPerspective.Users.All(o => o.Id != gameUser.Id))
        {
            var publicNewUser = ToPublicUser(gameUser);
            UpdateAllPerspectives(gameId, state =>
            {
                if (state.UserId == gameUser.Id) return state;
                return state with
                {
                    Users = [.. state.Users, publicNewUser]
                };
            });
        }
    }

    public void SetTime(string gameId, GameTime gameTime)
    {
        UpdateAllPerspectives(gameId, state => state with { GameTime = gameTime });
    }

    public bool UpdateUser(string gameId,
        string affectedUserId,
        UserType? userType = null,
        bool? isPlaying = null)
    {
        bool updated = false;

        GamePerspective UpdateFunction(GamePerspective state)
        {
            var user = state.Users.FirstOrDefault(u => u.Id == affectedUserId);
            if (user is null || (userType == null || user.UserType == userType) && (isPlaying == null || user.IsPlaying == isPlaying)) return state;
            updated = true;

            var updatedUser = user with { UserType = userType ?? user.UserType, IsPlaying = isPlaying ?? user.IsPlaying };

            return state with { Users = state.Users.Select(u => u.Id == affectedUserId ? updatedUser : u).ToList() };
        }

        UpdateAllPerspectives(gameId, UpdateFunction);

        return updated;
    }


    private void UpdateAllPerspectives(string gameId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        var perspectiveIds = _store.Keys.Where(key =>
                key is var (gId, _) && gId == gameId)
            .Select(o => o.Item2).ToList();
        foreach (var userId in perspectiveIds)
        {
            TryUpdate(gameId, userId, updateFunction);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "This just runs a delegate, the the value not found issue is covered in the calling functions")]
    private void TryUpdate(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        _store.AddOrUpdate((gameId, userId),
            addValueFactory: _ => throw new InvalidOperationException("Key should exist"),
            updateValueFactory: (_, existing) => updateFunction(existing)
        );
    }

    private GameUser ToPublicUser(GameUser user) =>
        new(user.Id)
        {
            UserType = user.UserType,
            IsPlaying = user.IsPlaying
        };
}