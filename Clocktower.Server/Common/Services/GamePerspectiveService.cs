namespace Clocktower.Server.Common.Services;

public class GamePerspectiveService(IGamePerspectiveStore store) : IGamePerspectiveService
{
    public IEnumerable<GamePerspective> GetAll() => store.GetAll();

    public bool GameExists(string gameId) => store.GetAllPerspectivesForGame(gameId).Any();

    public GamePerspective? GetFirstPerspective(string gameId)
    {
        var gamePerspectives = store.GetAllPerspectivesForGame(gameId);
        return gamePerspectives.FirstOrDefault();
    }

    public GamePerspective? GetPerspective(string gameId, string userId)
    {
        var personal = store.Get(gameId, userId);
        if (personal != null) return personal;

        var omniscient = store.Get(gameId, IGamePerspectiveStore.OmniscientKey);
        if (omniscient is null) return null;
        var user = omniscient.Users.FirstOrDefault(u => u.Id == userId);
        if (IsOmniscient(user?.UserType)) return omniscient;
        return null;
    }

    public void RemoveUserFromGame(string gameId, string userId)
    {
        store.Remove(gameId, userId);

        store.UpdateAllPerspectives(gameId, state => state with
        {
            Users = state.Users.Where(u => u.Id != userId).ToList()
        });

        var perspective = GetFirstPerspective(gameId);
        if (perspective is null) return;

        var sortedUsers = perspective.Players.OrderBy(u => u.SeatingPosition).ToList();

        store.UpdateAllPerspectives(gameId, state =>
        {
            var updatedUsers = state.Users.Select(user =>
            {
                var newPosition = sortedUsers.FindIndex(u => u.Id == user.Id);
                return newPosition >= 0 ? user with { SeatingPosition = newPosition } : user;
            }).ToList();

            return state with { Users = updatedUsers };
        });
    }

    public bool UpdatePublicUser(string gameId, string userId, PublicGameUserUpdate update)
    {
        bool updated = false;

        if (update.UserType != null) HandleUserTypeTransition(gameId, userId, update.UserType.Value);

        store.UpdateAllPerspectives(gameId, state =>
        {
            var user = state.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null || !UserHasPublicChanges(user, update)) return state;

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

            return state with { Users = state.Users.Select(u => u.Id == userId ? updatedUser : u).ToList() };
        });

        return updated;
    }

    public bool UpdatePrivateUser(string gameId, string userId, PrivateGameUserUpdate update)
    {
        bool updated = false;

        store.UpdateUserInOwnAndOmniscientPerspectives(gameId, userId, state =>
        {
            var user = state.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null || !UserHasPrivateChanges(user, update)) return state;

            updated = true;

            var updatedUser = user with
            {
                Role = update.Role ?? user.Role
            };

            return state with { Users = state.Users.Select(u => u.Id == userId ? updatedUser : u).ToList() };
        });

        return updated;
    }


    public bool AddUserToGame(string gameId, GameUser gameUser)
    {
        var existingPerspective = GetFirstPerspective(gameId);
        if (existingPerspective is null)
            return false;
        if (existingPerspective.Users.Any(u => u.Id == gameUser.Id))
            return false;

        if (IsOmniscient(gameUser.UserType))
        {
            AddOmniscientUserToGame(gameId, gameUser, existingPerspective);
        }
        else
        {
            AddPlayerToGame(gameId, gameUser, existingPerspective);
        }

        AddUserToAllPerspectives(gameId, gameUser);
        return true;
    }

    public GamePerspective? InitializeGame(string gameId, string guildId, GameUser initialUser)
    {
        if (GameExists(gameId)) return null;

        var perspectiveKey = IsOmniscient(initialUser.UserType) ? IGamePerspectiveStore.OmniscientKey : initialUser.Id;

        var initialPerspective = new GamePerspective(gameId, perspectiveKey, guildId, initialUser, DateTime.UtcNow)
        {
            Users = [initialUser]
        };

        var addSuccess = store.Add(gameId, perspectiveKey, initialPerspective);
        return addSuccess ? initialPerspective : null;
    }

    public IEnumerable<GamePerspective> GetAllPerspectivesForGame(string gameId) => store.GetAllPerspectivesForGame(gameId);

    public int GetNextAvailableSeatingPosition(string gameId)
    {
        var perspective = GetFirstPerspective(gameId);
        if (perspective is null) return UserDto.NoSeatingPosition;
        var currentPlayers = perspective.Players.ToArray();
        if (!currentPlayers.Any()) return 0;

        var maxPosition = currentPlayers.Max(u => u.SeatingPosition);
        return maxPosition + 1;
    }

    public IEnumerable<GamePerspective> GetUserGames(string userId)
    {
        var all = store.GetAll();
        return all.Where(p =>
            p.UserId == userId ||
            (p.UserId == IGamePerspectiveStore.OmniscientKey && p.Users.Any(u => u.Id == userId && IsOmniscient(u.UserType)))
        ).DistinctBy(p => p.Id);
    }

    public IEnumerable<string> GetGuildGameIds(string guildId)
    {
        var all = store.GetAll();
        return all.Where(g => g.GuildId == guildId).Select(g => g.Id).Distinct();
    }

    public bool RemoveGame(string gameId)
    {
        var perspectives = store.GetAllPerspectivesForGame(gameId).ToArray();
        foreach (var key in perspectives)
        {
            store.Remove(key.Id, key.UserId);
        }

        return perspectives.Any();
    }

    public void SetTime(string gameId, GameTime gameTime)
    {
        store.UpdateAllPerspectives(gameId, state => state with { GameTime = gameTime });
    }

    private static bool IsOmniscient(UserType? userType)
    {
        if (userType is null) return false;
        return userType is UserType.StoryTeller or UserType.Spectator;
    }

    private static bool UserHasPublicChanges(GameUser user, PublicGameUserUpdate update) =>
        (update.UserType != null && user.UserType != update.UserType) ||
        (update.IsPlaying != null && user.IsPlaying != update.IsPlaying) ||
        (update.SeatingPosition != null && user.SeatingPosition != update.SeatingPosition) ||
        (update.HasVoteToken != null && user.HasVoteToken != update.HasVoteToken) ||
        (update.IsDead != null && user.IsDead != update.IsDead) ||
        (update.IsMarked != null && user.IsMarked != update.IsMarked);

    private static bool UserHasPrivateChanges(GameUser user, PrivateGameUserUpdate update) =>
        update.Role != null && user.Role != update.Role;

    private void HandleUserTypeTransition(string gameId, string userId, UserType newUserType)
    {
        var currentPerspective = GetPerspective(gameId, userId);
        if (currentPerspective is null) return;

        var currentUser = currentPerspective.Users.FirstOrDefault(u => u.Id == userId);

        var wasOmniscient = IsOmniscient(currentUser!.UserType);
        var isNowOmniscient = IsOmniscient(newUserType);
        if (wasOmniscient == isNowOmniscient)
            return;

        if (isNowOmniscient)
        {
            var oldPerspective = store.Get(gameId, userId);
            if (oldPerspective != null)
            {
                store.Remove(gameId, userId);
                store.Add(gameId, IGamePerspectiveStore.OmniscientKey, oldPerspective with { UserId = IGamePerspectiveStore.OmniscientKey });
            }
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
                store.Add(gameId, userId, personalPerspective);
            }
        }
    }

    private void AddOmniscientUserToGame(string gameId, GameUser gameUser, GamePerspective templatePerspective)
    {
        var existingOmniscient = store.Get(gameId, IGamePerspectiveStore.OmniscientKey);
        if (existingOmniscient is null)
        {
            var omniscientPerspective = CreateOmniscientPerspective(templatePerspective);
            store.Add(gameId, IGamePerspectiveStore.OmniscientKey, omniscientPerspective);
        }

        store.TryUpdate(gameId, IGamePerspectiveStore.OmniscientKey, state =>
            state with { Users = [.. state.Users, gameUser] }
        );
    }

    private static GamePerspective CreateOmniscientPerspective(GamePerspective template)
    {
        return template with
        {
            UserId = IGamePerspectiveStore.OmniscientKey,
            Users = template.Users.ToList()
        };
    }

    private void AddPlayerToGame(string gameId, GameUser gameUser, GamePerspective templatePerspective)
    {
        var playerPerspective = templatePerspective with
        {
            UserId = gameUser.Id,
            Users = templatePerspective.Users.Select(ToPublicUser).Append(gameUser).ToList()
        };

        store.Add(gameId, gameUser.Id, playerPerspective);
    }

    private void AddUserToAllPerspectives(string gameId, GameUser gameUser)
    {
        var publicUser = ToPublicUser(gameUser);

        store.UpdateAllPerspectives(gameId, state =>
        {
            if (state.Users.Any(u => u.Id == gameUser.Id)) return state;

            var userToAdd = ShouldReceiveFullUserData(state.UserId, gameUser.Id)
                ? gameUser
                : publicUser;

            return state with { Users = [.. state.Users, userToAdd] };
        });
    }


    private static bool ShouldReceiveFullUserData(string perspectiveUserId, string targetUserId)
    {
        return perspectiveUserId == IGamePerspectiveStore.OmniscientKey || perspectiveUserId == targetUserId;
    }

    private static GameUser ToPublicUser(GameUser user) =>
        new(user.Id)
        {
            UserType = user.UserType,
            IsPlaying = user.IsPlaying,
            SeatingPosition = user.SeatingPosition,
            IsDead = user.IsDead,
            IsMarked = user.IsMarked,
            HasVoteToken = user.HasVoteToken
        };
}