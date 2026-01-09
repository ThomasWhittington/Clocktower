using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Data.Stores;

public class GamePerspectiveStore : IGamePerspectiveStore
{
    public const string OmniscientKey = "omniscient";

    private readonly ConcurrentDictionary<(string, string), GamePerspective> _store = new();

    public bool GameExists(string gameId) => _store.Any(o => o.Key.Item1 == gameId);

    public void Clear() => _store.Clear();

    public GamePerspective? Get(string gameId, string userId)
    {
        if (_store.TryGetValue((gameId, userId), out var perspective)) return perspective;

        if (_store.TryGetValue((gameId, OmniscientKey), out var omniscient))
        {
            var user = omniscient.Users.FirstOrDefault(u => u.Id == userId);
            if (IsOmniscient(user?.UserType)) return omniscient;
        }

        return null;
    }

    public bool Set(GamePerspective perspective)
    {
        var user = perspective.Users.FirstOrDefault(u => u.Id == perspective.UserId);

        if (IsOmniscient(user?.UserType))
        {
            return _store.TryAdd((perspective.Id, OmniscientKey), perspective);
        }

        return _store.TryAdd((perspective.Id, perspective.UserId), perspective);
    }

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
        return _store.Values
            .Where(p =>
                p.UserId == userId ||
                (p.UserId == OmniscientKey && p.Users.Any(u => u.Id == userId && IsOmniscient(u.UserType)))
            ).DistinctBy(p => p.Id);
    }

    public IEnumerable<GamePerspective> GetAll() => _store.Values;

    public GamePerspective? GetFirstPerspective(string gameId) => _store.FirstOrDefault(kvp => kvp.Key.Item1 == gameId).Value;

    public void AddUserToGame(string gameId, GameUser gameUser)
    {
        var existingPerspective = GetFirstPerspective(gameId);
        if (existingPerspective is null) return;

        if (existingPerspective.Users.Any(u => u.Id == gameUser.Id)) return;

        if (IsOmniscient(gameUser.UserType))
        {
            AddOmniscientUserToGame(gameId, gameUser, existingPerspective);
        }
        else
        {
            AddPlayerToGame(gameId, gameUser, existingPerspective);
        }

        AddUserToAllPerspectives(gameId, gameUser);
    }

    public void RemoveUserFromGame(string gameId, string userId)
    {
        RemovePerspective(gameId, userId);

        UpdateAllPerspectives(gameId, state => state with
        {
            Users = state.Users.Where(u => u.Id != userId).ToList()
        });

        var perspective = GetFirstPerspective(gameId);
        if (perspective is null) return;

        var sortedUsers = perspective.Players
            .OrderBy(u => u.SeatingPosition)
            .ToList();

        UpdateAllPerspectives(gameId, state =>
        {
            var updatedUsers = state.Users.Select(user =>
            {
                var newPosition = sortedUsers.FindIndex(u => u.Id == user.Id);
                return newPosition >= 0 ? user with { SeatingPosition = newPosition } : user;
            }).ToList();

            return state with { Users = updatedUsers };
        });
    }

    public void SetTime(string gameId, GameTime gameTime)
    {
        UpdateAllPerspectives(gameId, state => state with { GameTime = gameTime });
    }

    public bool UpdatePublicUser(string gameId, string affectedUserId, GameUserUpdate update)
    {
        bool updated = false;

        if (update.UserType != null) HandleUserTypeTransition(gameId, affectedUserId, update.UserType.Value);

        UpdateAllPerspectives(gameId, state =>
        {
            var user = state.Users.FirstOrDefault(u => u.Id == affectedUserId);
            if (user is null || !HasChanges(user, update)) return state;

            updated = true;

            var updatedUser = user with
            {
                UserType = update.UserType ?? user.UserType,
                IsPlaying = update.IsPlaying ?? user.IsPlaying,
                SeatingPosition = update.SeatingPosition ?? user.SeatingPosition,
                IsDead = update.IsDead ?? user.IsDead,
                IsMarked = update.IsMarked ?? user.IsMarked,
                HasVoteToken = update.HasVoteToken ?? user.HasVoteToken
            };

            return state with { Users = state.Users.Select(u => u.Id == affectedUserId ? updatedUser : u).ToList() };
        });

        return updated;
    }

    public int GetNextAvailableSeatingPosition(string gameId)
    {
        var perspective = GetFirstPerspective(gameId);
        if (perspective is null) return UserDto.NoSeatingPosition;
        var currentPlayers = perspective.Players.ToArray();
        if (!currentPlayers.Any()) return 0;

        var maxPosition = currentPlayers.Max(u => u.SeatingPosition);
        return maxPosition + 1;
    }

    public void SetUserRole(string gameId, string userId, Role role)
    {
        UpdateUserInOwnAndOmniscientPerspectives(gameId, userId, state => state with
        {
            Users = state.Users.Select(user => user.Id == userId ? user with { Role = role } : user).ToList()
        });
    }

    private void UpdateUserInOwnAndOmniscientPerspectives(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        if (_store.ContainsKey((gameId, userId)))
        {
            TryUpdate(gameId, userId, updateFunction);
        }

        if (_store.ContainsKey((gameId, OmniscientKey)))
        {
            TryUpdate(gameId, OmniscientKey, updateFunction);
        }
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

    [ExcludeFromCodeCoverage(Justification = "This just runs a delegate, the value not found issue is covered in the calling functions")]
    private void TryUpdate(string gameId, string userId, Func<GamePerspective, GamePerspective> updateFunction)
    {
        _store.AddOrUpdate((gameId, userId),
            addValueFactory: _ => throw new InvalidOperationException("Key should exist"),
            updateValueFactory: (_, existing) => updateFunction(existing)
        );
    }

    private void HandleUserTypeTransition(string gameId, string userId, UserType newUserType)
    {
        var currentPerspective = Get(gameId, userId);
        if (currentPerspective is null) return;

        var currentUser = currentPerspective.Users.FirstOrDefault(u => u.Id == userId);
        if (currentUser is null) return;

        var wasOmniscient = IsOmniscient(currentUser.UserType);
        var isNowOmniscient = IsOmniscient(newUserType);
        if (wasOmniscient == isNowOmniscient) return;

        if (isNowOmniscient)
        {
            _store.TryRemove((gameId, userId), out var oldPerspective);
            _store.TryAdd((gameId, OmniscientKey), oldPerspective! with { UserId = OmniscientKey });
        }
        else
        {
            var templatePerspective = GetFirstPerspective(gameId);
            if (templatePerspective != null)
            {
                var personalPerspective = templatePerspective with
                {
                    UserId = userId,
                    Users = templatePerspective.Users.Select(u => u.Id == userId ? u : ToPublicUser(u)).ToList()
                };
                _store.TryAdd((gameId, userId), personalPerspective);
            }
        }
    }

    private GameUser ToPublicUser(GameUser user) =>
        new(user.Id)
        {
            UserType = user.UserType,
            IsPlaying = user.IsPlaying,
            SeatingPosition = user.SeatingPosition,
            IsDead = user.IsDead,
            IsMarked = user.IsMarked,
            HasVoteToken = user.HasVoteToken
        };

    private static bool HasChanges(GameUser user, GameUserUpdate update) =>
        (update.UserType != null && user.UserType != update.UserType) ||
        (update.IsPlaying != null && user.IsPlaying != update.IsPlaying) ||
        (update.SeatingPosition != null && user.SeatingPosition != update.SeatingPosition) ||
        (update.HasVoteToken != null && user.HasVoteToken != update.HasVoteToken) ||
        (update.IsDead != null && user.IsDead != update.IsDead) ||
        (update.IsMarked != null && user.IsMarked != update.IsMarked);

    private static bool IsOmniscient(UserType? userType)
    {
        if (userType is null) return false;
        return userType is UserType.StoryTeller or UserType.Spectator;
    }

    private void AddOmniscientUserToGame(string gameId, GameUser gameUser, GamePerspective templatePerspective)
    {
        _store.GetOrAdd((gameId, OmniscientKey), _ => CreateOmniscientPerspective(templatePerspective));

        TryUpdate(gameId, OmniscientKey, state =>
        {
            if (state.Users.Any(u => u.Id == gameUser.Id)) return state;
            return state with { Users = [.. state.Users, gameUser] };
        });
    }

    private void AddPlayerToGame(string gameId, GameUser gameUser, GamePerspective templatePerspective)
    {
        var playerPerspective = templatePerspective with
        {
            UserId = gameUser.Id,
            Users = templatePerspective.Users.Select(ToPublicUser).Append(gameUser).ToList()
        };

        _store.TryAdd((gameId, gameUser.Id), playerPerspective);
    }

    private void AddUserToAllPerspectives(string gameId, GameUser gameUser)
    {
        var publicUser = ToPublicUser(gameUser);

        UpdateAllPerspectives(gameId, state =>
        {
            if (state.Users.Any(u => u.Id == gameUser.Id)) return state;

            var userToAdd = ShouldReceiveFullUserData(state.UserId, gameUser.Id)
                ? gameUser
                : publicUser;

            return state with { Users = [.. state.Users, userToAdd] };
        });
    }

    private static GamePerspective CreateOmniscientPerspective(GamePerspective template)
    {
        return template with
        {
            UserId = OmniscientKey,
            Users = template.Users.ToList()
        };
    }

    private static bool ShouldReceiveFullUserData(string perspectiveUserId, string targetUserId)
    {
        return perspectiveUserId == OmniscientKey || perspectiveUserId == targetUserId;
    }
}