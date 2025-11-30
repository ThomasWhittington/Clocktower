namespace Clocktower.Server.Game.Services;

public interface IGameStateService
{
    (bool success, GameState? gameState, string message) StartNewGame(string guildId, string gameId, ulong userId);
    (bool success, string message) DeleteGame(string gameId);
    (bool success, GameState? gameState, string message) GetGame(string gameId);
    IEnumerable<GameState> GetGames();
    IEnumerable<GameState> GetGuildGames(string guildId);
    IEnumerable<MiniGameState> GetPlayerGames(string userId);
    (bool success, string message) LoadDummyData(string filePath = "dummyState.json");
    Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime);
}