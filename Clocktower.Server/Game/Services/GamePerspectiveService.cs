using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket;

namespace Clocktower.Server.Game.Services;

public class GamePerspectiveService(IDiscordBot bot, IGamePerspectiveStore gamePerspectiveStore, IDiscordTownManager discordTownManager, IFileSystem fileSystem, INotificationService notificationService) : IGamePerspectiveService
{
    public IEnumerable<GamePerspective> GetGames() => gamePerspectiveStore.GetAll();

    public IEnumerable<MiniGamePerspective> GetPlayerGames(string userId)
    {
        var playerGames = gamePerspectiveStore.GetUserGames(userId);
        var miniGamePerspectives = playerGames.Select(o => new MiniGamePerspective(o.Id, o.CreatedBy, o.CreatedDate));
        return miniGamePerspectives;
    }

    public (bool success, string message) LoadDummyData(string filePath = "dummyState.json")
    {
        try
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
    }

    public (bool success, IEnumerable<GamePerspective> perspectives, string message) GetGamePerspectives(string gameId)
    {
        var game = gamePerspectiveStore.GetAllPerspectivesForGame(gameId).ToArray();

        return game.Any()
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
        var guild = bot.GetGuild(guildId);
        if (guild is null) return (false, null, "Couldn't find guild");
        var user = guild.GetUser(userId);
        if (user is null) return (false, null, "Couldn't find user");

        var gameUser = user.AsGameUser();
        gameUser.UserType = UserType.StoryTeller;
        var townUser = user.AsTownUser();
        discordTownManager.UpdateUserIdentity(townUser);
        var newGamePerspective = new GamePerspective(gameId, userId, guildId, gameUser, DateTime.UtcNow)
        {
            Users = [gameUser]
        };

        bool addSuccessful = gamePerspectiveStore.Set(newGamePerspective);

        return addSuccessful
            ? (true, newGamePerspective, "Game started successfully")
            : (false, null, $"Perspective for user '{userId}' for game '{gameId}' already exists");
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

    public Result<IEnumerable<UserDto>> GetAvailableGameUsers(string gameId)
    {
        var game = gamePerspectiveStore.GetFirstPerspective(gameId);
        if (game is null) return Result.Fail<IEnumerable<UserDto>>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(game.GuildId);
        if (guild is null) return Result.Fail<IEnumerable<UserDto>>(Errors.InvalidGuildId());

        var gameUsersIds = game.Users.Select(o => o.Id).ToHashSet();

        var users = guild.Users
            .Where(u => !u.IsBot && !gameUsersIds.Contains(u.Id))
            .Select(u => UserDto.FromTownUser(u.AsTownUser()))
            .ToArray();

        return Result.Ok<IEnumerable<UserDto>>(users);
    }

    public async Task<Result<string>> AddUserToGame(string gameId, string userId)
    {
        var gamePerspective = gamePerspectiveStore.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        var townUser = user.AsTownUser();
        discordTownManager.UpdateUserIdentity(townUser);
        var gameUser = user.AsGameUser(gamePerspective);
        gameUser.UserType = UserType.Player;
        gameUser.SeatingPosition = gamePerspectiveStore.GetNextAvailableSeatingPosition(gameId);
        //TODO remove test data
        gameUser.HasVoteToken = Random.Shared.Next(0, 2) == 1;
        gameUser.IsDead = Random.Shared.Next(0, 2) == 1;
        gameUser.IsMarked = Random.Shared.Next(0, 2) == 1;
        gamePerspectiveStore.AddUserToGame(gameId, gameUser);

        await notificationService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok($"{user.DisplayName} added to game: {gameId}");
    }

    public async Task<Result<string>> RemoveUserFromGame(string gameId, string userId)
    {
        var gamePerspective = gamePerspectiveStore.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        gamePerspectiveStore.RemoveUserFromGame(gameId, userId);

        await notificationService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok($"{user.DisplayName} removed from game: {gameId}");
    }

    public async Task<Result<string[]>> RandomiseSeatingPositions(string gameId)
    {
        var gamePerspective = gamePerspectiveStore.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string[]>(Errors.GameNotFound(gameId));

        var shuffledPlayers = gamePerspective.Players.OrderBy(_ => Guid.NewGuid()).ToArray();

        foreach (GameUser shuffledPlayer in shuffledPlayers)
        {
            gamePerspectiveStore.UpdatePublicUser(
                gameId,
                shuffledPlayer.Id,
                new GameUserUpdate
                {
                    SeatingPosition = Array.IndexOf(shuffledPlayers, shuffledPlayer)
                }
            );
        }

        await notificationService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok(shuffledPlayers.Select(u => u.Id).ToArray());
    }

    public async Task<Result<string>> SwapSeatingPositions(string gameId, string userId1, string userId2)
    {
        var gamePerspective = gamePerspectiveStore.GetFirstPerspective(gameId);
        if (gamePerspective is null)
            return Result.Fail<string>(Errors.GameNotFound(gameId));

        var user1 = gamePerspective.Users.FirstOrDefault(u => u.Id == userId1);
        if (user1 is null) return Result.Fail<string>(Errors.UserNotFound(userId1));

        var user2 = gamePerspective.Users.FirstOrDefault(u => u.Id == userId2);
        if (user2 is null) return Result.Fail<string>(Errors.UserNotFound(userId2));

        var tempPosition = user1.SeatingPosition;
        gamePerspectiveStore.UpdatePublicUser(gameId, userId1, new GameUserUpdate
        {
            SeatingPosition = user2.SeatingPosition
        });
        gamePerspectiveStore.UpdatePublicUser(gameId, userId2, new GameUserUpdate
        {
            SeatingPosition = tempPosition
        });
        await notificationService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok("Users swapped");
    }

    public Task<Result<string>> SetPlayerIsDead(string gameId, string userId, bool isDead)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string>> SetPlayerHasVoteToken(string gameId, string userId, bool hasVoteToken)
    {
        throw new NotImplementedException();
    }
}