using System.Collections.Concurrent;
using Clocktower.Server.Data.Extensions;

namespace Clocktower.Server.Data.Stores;

public class GameStateStore : IGameStateStore
{
    private readonly ConcurrentDictionary<string, GameState> _store = new();

    public void Clear() => _store.Clear();

    public GameState? Get(string gameId) =>
        _store.TryGetValue(gameId, out var state) ? state : null;

    public bool Remove(string gameId) => _store.TryRemove(gameId, out _);

    public bool Set(GameState state)
    {
        var currentValue = Get(state.Id);
        if (currentValue is not null) return false;
        _store[state.Id] = state;
        return true;
    }


    public bool TryUpdate(string gameId, Func<GameState, GameState> updateFunction)
    {
        if (!_store.TryGetValue(gameId, out var existing)) return false;

        _store[gameId] = updateFunction(existing);
        return true;
    }

    public IEnumerable<GameState> GetGuildGames(string guildId)
    {
        return GetAll().Where(game => game.GuildId == guildId);
    }

    public IEnumerable<GameState> GetGuildGames(ulong guildId) => GetGuildGames(guildId.ToString());

    public IEnumerable<GameState> GetUserGames(string userId)
    {
        return GetAll().Where(game => game.Users.Select(o => o.Id).Contains(userId));
    }

    public IEnumerable<GameState> GetAll() => _store.Values;

    public void AddUserToGame(string gameId, GameUser gameUser)
    {
        TryUpdate(gameId, state =>
        {
            state.Users.Add(gameUser);
            return state;
        });
    }

    public void SetTime(string gameId, GameTime gameTime)
    {
        TryUpdate(gameId, state =>
        {
            state.GameTime = gameTime;
            return state;
        });
    }

    public GameState? UpdateUser(string gameId, ulong userId, UserType? userType = null, bool? isPlaying = null)
    {
        TryUpdate(gameId, UpdateFunction);
        return Get(gameId);

        GameState UpdateFunction(GameState state)
        {
            var user = state.Users.GetById(userId);
            if (user is null) return state;

            if (userType.HasValue)
            {
                user.UserType = userType.Value;
            }

            if (isPlaying.HasValue)
            {
                user.IsPlaying = isPlaying.Value;
            }

            return state;
        }
    }
}