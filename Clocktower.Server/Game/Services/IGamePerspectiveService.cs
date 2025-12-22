namespace Clocktower.Server.Game.Services;

public interface IGamePerspectiveService
{
    (bool success, GamePerspective? gamePerspective, string message) StartNewGame(string guildId, string gameId, string userId);
    (bool success, string message) DeleteGame(string gameId);
    (bool success, IEnumerable<GamePerspective> perspectives, string message) GetGame(string gameId);
    IEnumerable<GamePerspective> GetGames();
    IEnumerable<MiniGamePerspective> GetPlayerGames(string userId);
    (bool success, string message) LoadDummyData(string filePath = "dummyState.json");
    Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime);
    Task<Result<GamePerspectiveDto>> GetPlayerGamePerspectiveDto(string gameId, string userId);
}