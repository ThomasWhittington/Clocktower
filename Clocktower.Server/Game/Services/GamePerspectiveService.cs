using System.IO.Abstractions;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Game.Services;

public class GamePerspectiveService(IDiscordBot bot, IGamePerspectiveStore gamePerspectiveStore, IFileSystem fileSystem, INotificationService notificationService) : IGamePerspectiveService
{
    public IEnumerable<GamePerspective> GetGames() =>
        gamePerspectiveStore.GetAll();

    public IEnumerable<GamePerspective> GetGuildGames(string guildId) =>
        gamePerspectiveStore.GetGuildGames(guildId);

    public IEnumerable<MiniGamePerspective> GetPlayerGames(string userId)
    {
        var playerGames = gamePerspectiveStore.GetUserGames(userId);
        var miniGamePerspectives = playerGames.Select(o => new MiniGamePerspective(o.Id, o.CreatedBy, o.CreatedDate));
        return miniGamePerspectives;
    }

    public (bool success, string message) LoadDummyData(string filePath = "dummyState.json")
    {
        //TODO implement new dummy data once rework of GamePerspective is complete
        throw new NotImplementedException();

        /*try
        {
            var json = fileSystem.File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            var games = JsonSerializer.Deserialize<GamePerspective[]>(json, options);
            if (games == null)
            {
                return (false, "Failed to deserialize json");
            }

            gamePerspectiveStore.Clear();
            foreach (var gamePerspective in games)
            {
                gamePerspectiveStore.Set(gamePerspective);
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
        */
    }

    public (bool success, IEnumerable<GamePerspective> perspectives, string message) GetGame(string gameId)
    {
        var game = gamePerspectiveStore.GetAllPerspectivesForGame(gameId);

        return game is not null
            ? (true, game, "Game retrieved successfully")
            : (false, [], $"Game ID '{gameId}' not found");
    }


    public (bool success, string message) DeleteGame(string gameId)
    {
        bool deleteSuccessful = gamePerspectiveStore.RemoveGame(gameId);

        return deleteSuccessful
            ? (true, "Game deleted successfully")
            : (false, $"Game ID '{gameId}' failed to be deleted");
    }

    public (bool success, GamePerspective? gamePerspective, string message) StartNewGame(string guildId, string gameId, string userId)
    {
        var user = bot.GetUser(userId);
        if (user is null) return (false, null, "Couldn't find user");
        var gameUser = user.AsGameUser();
        gameUser.UserType = UserType.StoryTeller;
        var newGamePerspective = new GamePerspective(gameId, guildId, gameUser, DateTime.UtcNow)
        {
            Users = [gameUser]
        };

        bool addSuccessful = gamePerspectiveStore.Set(newGamePerspective, userId);

        return addSuccessful
            ? (true, newGamePerspective, "Game started successfully")
            : (false, null, $"Game Id '{gameId}' already exists");
    }

    public async Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime)
    {
        try
        {
            var gamePerspective = gamePerspectiveStore.GameExists(gameId);
            if (!gamePerspective) return (false, "Game not found");

            gamePerspectiveStore.SetTime(gameId, gameTime);
            await notificationService.BroadcastTownTime(gameId, gameTime);
            return (true, $"Time set to {gameTime}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Task<Result<GamePerspectiveDto>> GetPlayerGamePerspective(string gameId, string userId)
    {
        //TODO implement GetPlayerGamePerspective
        throw new NotImplementedException();
    }
}