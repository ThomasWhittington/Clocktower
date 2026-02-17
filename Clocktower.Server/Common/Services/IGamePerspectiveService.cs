namespace Clocktower.Server.Common.Services;

public interface IGamePerspectiveService
{
    IEnumerable<GamePerspective> GetAll();
    bool GameExists(string gameId);
    bool RemoveGame(string gameId);
    GamePerspective? GetPerspective(string gameId, string userId);
    GamePerspective? GetFirstPerspective(string gameId);
    void RemoveUserFromGame(string gameId, string userId);
    bool UpdatePublicUser(string gameId, string userId, PublicGameUserUpdate update);
    bool UpdatePrivateUser(string gameId, string userId, PrivateGameUserUpdate update);
    bool SetRoleOnPerspective(string gameId, string userId, string targetUserId, Role? role);
    bool SetRoleOnAllPerspectives(string gameId, string targetUserId, Role? role);
    bool AddUserToGame(string gameId, GameUser gameUser);
    GamePerspective? InitializeGame(string gameId, string guildId, GameUser initialUser);
    IEnumerable<GamePerspective> GetAllPerspectivesForGame(string gameId);
    int GetNextAvailableSeatingPosition(string gameId);
    IEnumerable<GamePerspective> GetUserGames(string userId);
    IEnumerable<string> GetGuildGameIds(string guildId);
    void SetTime(string gameId, GameTime gameTime);
    void SetScript(string gameId, Script script);
    bool UpdateDraftRole(string gameId, string userId, Role? draftRole);
    void CommitDraftRoles(string gameId);
    void ResetNominationSession(string gameId);
    bool AddReminderForUserOnPerspective(string gameId, string userId, string targetUserId, ReminderToken reminder);
    bool RemoveReminderForUserOnPerspective(string gameId, string userId, string targetUserId, ReminderToken reminder);
}