using System.IO.Abstractions;
using System.Text.Json;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Extensions;

namespace Clocktower.Server.Game.Services;

public class GameStateService(IDiscordBot bot, IGameStateStore gameStateStore, IFileSystem fileSystem) : IGameStateService
{
    public IEnumerable<GameState> GetGames() =>
        gameStateStore.GetAll();

    public IEnumerable<GameState> GetGuildGames(string guildId) =>
        gameStateStore.GetGuildGames(guildId);

    public IEnumerable<MiniGameState> GetPlayerGames(string userId)
    {
        var playerGames = gameStateStore.GetUserGames(userId);
        var miniGameStates = playerGames.Select(o => new MiniGameState(o.Id, o.CreatedBy, o.CreatedDate));
        return miniGameStates;
    }

    public (bool success, string message) LoadDummyData(string filePath = "dummyState.json")
    {
        try
        {
            var json = fileSystem.File.ReadAllText(filePath);
            var games = JsonSerializer.Deserialize<GameState[]>(json);
            if (games == null)
            {
                return (false, "Failed to deserialize json");
            }

            gameStateStore.Clear();
            foreach (var gameState in games)
            {
                gameStateStore.Set(gameState.Id, gameState);
            }

            return (true, "Loaded dummy data");
        }
        catch (JsonException)
        {
            return (false, "Failed to deserialize json");
        }
        catch (FileNotFoundException)
        {
            return (false, "File not found");
        }
        catch (Exception ex)
        {
            return (false, $"Error loading dummy data: {ex.Message}");
        }
    }

    public (bool success, GameState? gameState, string message) GetGame(string gameId)
    {
        var game = gameStateStore.Get(gameId);

        return game is not null
            ? (true, game, "Game retrieved successfully")
            : (false, null, $"Game ID '{gameId}' not found");
    }


    public (bool success, string message) DeleteGame(string gameId)
    {
        bool deleteSuccessful = gameStateStore.Remove(gameId);

        return deleteSuccessful
            ? (true, "Game deleted successfully")
            : (false, $"Game ID '{gameId}' failed to be deleted");
    }

    public (bool success, GameState? gameState, string message) StartNewGame(string guildId, string gameId, ulong userId)
    {
        var user = bot.GetUser(userId);
        if (user is null) return (false, null, "Couldn't find user");
        var gameUser = user.AsGameUser();
        gameUser.UserType = UserType.StoryTeller;
        var newGameState = new GameState
        {
            Id = gameId,
            GuildId = guildId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = gameUser,
            Users = [gameUser]
        };

        bool addSuccessful = gameStateStore.Set(gameId, newGameState);

        return addSuccessful
            ? (true, newGameState, "Game started successfully")
            : (false, null, $"Game Id '{gameId}' already exists");
    }
}