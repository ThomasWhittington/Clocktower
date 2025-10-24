using System.Collections.Concurrent;
using System.Text.Json;
using Clocktower.Server.Data;

namespace Clocktower.Server.Game.Services;

public class GameStateService
{
    private readonly ConcurrentDictionary<string, GameState> _games = new();

    public GameState[] GetGames()
    {
        return _games.Values.ToArray();
    }

    public (bool success, string message) LoadDummyData()
    {
        var json = File.ReadAllText("dummyState.json");
        var games = JsonSerializer.Deserialize<GameState[]>(json);
        if (games == null)
        {
            return (false, "Failed to deserialize json");
        }

        _games.Clear();
        foreach (var gameState in games)
        {
            _games.TryAdd(gameState.Id, gameState);
        }

        return (true, "Loaded dummy data");
    }

    public (bool success, GameState? gameState, string message) GetGame(string gameId)
    {
        bool getSuccessful = _games.TryGetValue(gameId, out var game);

        return getSuccessful
            ? (true, game, "Game retrieved successfully")
            : (false, null, $"Game ID '{gameId}' not found");
    }


    public (bool success, string message) DeleteGame(string gameId)
    {
        bool deleteSuccessful = _games.TryRemove(gameId, out _);

        return deleteSuccessful
            ? (true, "Game deleted successfully")
            : (false, $"Game ID '{gameId}' failed to be deleted");
    }

    public (bool success, GameState? gameState, string message) StartNewGame(string gameId)
    {
        var newGameState = new GameState
        {
            Id = gameId
        };

        bool addSuccessful = _games.TryAdd(gameId, newGameState);

        return addSuccessful
            ? (true, newGameState, "Game started successfully")
            : (false, null, $"Game ID '{gameId}' already exists");
    }

    public (bool success, Player? newPlayer, string message, AddPlayerError error) AddPlayerToGame(string gameId, string playerName)
    {
        var gameResult = GetGame(gameId);
        if (!gameResult.success || gameResult.gameState == null)
            return (false, null, gameResult.message, AddPlayerError.GameNotFound);

        if (gameResult.gameState.IsFull)
            return (false, null, $"Game is full - cannot add more players", AddPlayerError.GameFull);

        if (gameResult.gameState.Players.Any(p => p.Name == playerName))
            return (false, null, $"Player '{playerName}' already exists in this game", AddPlayerError.PlayerAlreadyExists);

        var currentPlayerCount = gameResult.gameState.Players.Count;
        var newPlayer = new Player { Name = playerName, Id = currentPlayerCount };

        gameResult.gameState.Players.Add(newPlayer);

        return (true, newPlayer, "Player added successfully", AddPlayerError.NoError);
    }
}

public enum AddPlayerError
{
    NoError,
    GameNotFound,
    PlayerAlreadyExists,
    GameFull
}