using Clocktower.Server.Data.Dto;

namespace Clocktower.Server.Game.Services;

public interface IGamePerspectiveService
{
    (bool success, GamePerspective? gamePerspective, string message) StartNewGame(string guildId, string gameId, string userId);
    (bool success, string message) DeleteGame(string gameId);
    (bool success, IEnumerable<GamePerspective> perspectives, string message) GetGamePerspectives(string gameId);
    IEnumerable<GamePerspective> GetGames();
    IEnumerable<MiniGamePerspective> GetPlayerGames(string userId);
    (bool success, string message) LoadDummyData(string filePath = "dummyState.json");
    Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime);
    Result<IEnumerable<UserDto>> GetAvailableGameUsers(string gameId);
    Task<Result<string>> AddUserToGame(string gameId, string userId);
}