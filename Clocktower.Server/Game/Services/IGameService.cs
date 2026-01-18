namespace Clocktower.Server.Game.Services;

public interface IGameService
{
    (bool success, GamePerspective? gamePerspective, string message) StartNewGame(string guildId, string gameId, string userId);
    (bool success, string message) DeleteGame(string gameId);
    (bool success, IEnumerable<GamePerspective> perspectives, string message) GetGamePerspectives(string gameId);
    IEnumerable<GamePerspective> GetGames();
    IEnumerable<MiniGamePerspective> GetPlayerGames(string userId);
    Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime);
    Task<Result<Script>> SetScript(string gameId, ScriptSelect scriptSelect, string? json);
    Result<IEnumerable<UserDto>> GetAvailableGameUsers(string gameId);
    Task<Result<string>> AddUserToGame(string gameId, string userId);
    Task<Result<string>> RemoveUserFromGame(string gameId, string userId);
    Task<Result<string[]>> RandomiseSeatingPositions(string gameId);
    Task<Result<string>> SwapSeatingPositions(string gameId, string userId1, string userId2);
    Task<Result<string>> SetPlayerIsDead(string gameId, string userId, bool isDead);
    Task<Result<string>> SetPlayerHasVoteToken(string gameId, string userId, bool hasVoteToken);
    Task<Result<string>> SetPerspectiveRole(string gameId, string userId, string targetUserId, string? roleId);
    Task<Result<string>> SetRole(string gameId, string targetUserId, string? roleId);
    Task<Result<string>> SetDraftRole(string gameId, string targetUserId, string? roleId);
    Task<Result<string>> CommitDraftRoles(string gameId);
}