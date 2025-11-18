using System.Text.Json;
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
}