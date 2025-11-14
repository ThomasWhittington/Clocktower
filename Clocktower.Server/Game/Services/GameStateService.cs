using System.Text.Json;
using Clocktower.Server.Data.Stores;
using Clocktower.Server.Discord.Services;

namespace Clocktower.Server.Game.Services;

public class GameStateService(DiscordBotService bot)
{
    public IEnumerable<GameState> GetGames() =>
        GameStateStore.GetAll();

    public IEnumerable<GameState> GetGames(string guildId) =>
        GameStateStore.GetGames(guildId);

    public IEnumerable<MiniGameState> GetPlayerGames(string userId)
    {
        var playerGames = GameStateStore.GetPlayerGames(userId);
        var miniGameStates = playerGames.Select(o => new MiniGameState(o.Id, o.CreatedBy, o.CreatedDate));
        return miniGameStates;
    }

    public (bool success, string message) LoadDummyData()
    {
        var json = File.ReadAllText("dummyState.json");
        var games = JsonSerializer.Deserialize<GameState[]>(json);
        if (games == null)
        {
            return (false, "Failed to deserialize json");
        }

        GameStateStore.Clear();
        foreach (var gameState in games)
        {
            GameStateStore.Set(gameState.Id, gameState);
        }

        return (true, "Loaded dummy data");
    }

    public (bool success, GameState? gameState, string message) GetGame(string gameId)
    {
        var game = GameStateStore.Get(gameId);

        return game is not null
            ? (true, game, "Game retrieved successfully")
            : (false, null, $"Game ID '{gameId}' not found");
    }


    public (bool success, string message) DeleteGame(string gameId)
    {
        bool deleteSuccessful = GameStateStore.Remove(gameId);

        return deleteSuccessful
            ? (true, "Game deleted successfully")
            : (false, $"Game ID '{gameId}' failed to be deleted");
    }

    public (bool success, GameState? gameState, string message) StartNewGame(string guildId, string gameId, ulong userId)
    {
        var user = bot.Client.GetUser(userId);
        if (user is null) return (false, null, "Couldn't find user");
        var newGameState = new GameState
        {
            Id = gameId,
            GuildId = guildId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = user.AsGameUser()
        };

        bool addSuccessful = GameStateStore.Set(gameId, newGameState);

        return addSuccessful
            ? (true, newGameState, "Game started successfully")
            : (false, null, $"Game Id '{gameId}' already exists");
    }

    public (bool success, Player? newPlayer, string message, AddPlayerError error) AddPlayerToGame(string gameId, string playerName)
    {
        throw new NotImplementedException();

        /*
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
        */
    }
}

public enum AddPlayerError
{
    NoError,
    GameNotFound,
    PlayerAlreadyExists,
    GameFull
}