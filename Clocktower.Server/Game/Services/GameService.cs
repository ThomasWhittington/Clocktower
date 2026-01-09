using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Game.Services;

public class GameService(IDiscordBot bot, IGamePerspectiveService gamePerspectiveService, IDiscordTownManager discordTownManager, IGameBroadcastService gameBroadcastService) : IGameService
{
    public IEnumerable<GamePerspective> GetGames() => gamePerspectiveService.GetAll();

    public IEnumerable<MiniGamePerspective> GetPlayerGames(string userId)
    {
        var playerGames = gamePerspectiveService.GetUserGames(userId);
        var miniGamePerspectives = playerGames.Select(o => new MiniGamePerspective(o.Id, o.CreatedBy, o.CreatedDate));
        return miniGamePerspectives;
    }

    public (bool success, IEnumerable<GamePerspective> perspectives, string message) GetGamePerspectives(string gameId)
    {
        var game = gamePerspectiveService.GetAllPerspectivesForGame(gameId).ToArray();

        return game.Any()
            ? (true, game, "Game retrieved successfully")
            : (false, [], $"Game ID '{gameId}' not found");
    }


    public (bool success, string message) DeleteGame(string gameId)
    {
        bool deleteSuccessful = gamePerspectiveService.RemoveGame(gameId);

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

        var gameUser = user.AsGameUser() with { UserType = UserType.StoryTeller };
        discordTownManager.UpdateUserIdentity(user.AsTownUser());

        var newGamePerspective = gamePerspectiveService.InitializeGame(gameId, guildId, gameUser);

        return newGamePerspective is not null
            ? (true, newGamePerspective, "Game started successfully")
            : (false, null, $"Perspective for user '{userId}' for game '{gameId}' already exists");
    }

    public async Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime)
    {
        try
        {
            var gamePerspective = gamePerspectiveService.GameExists(gameId);
            if (!gamePerspective) return (false, "Game not found");

            gamePerspectiveService.SetTime(gameId, gameTime);
            await gameBroadcastService.BroadcastTimeUpdate(gameId, gameTime);
            return (true, $"Time set to {gameTime}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Result<IEnumerable<UserDto>> GetAvailableGameUsers(string gameId)
    {
        var game = gamePerspectiveService.GetFirstPerspective(gameId);
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
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        var townUser = user.AsTownUser();
        discordTownManager.UpdateUserIdentity(townUser);
        var gameUser = user.AsGameUser(gamePerspective);
        gameUser.UserType = UserType.Player;
        gameUser.SeatingPosition = gamePerspectiveService.GetNextAvailableSeatingPosition(gameId);

        gameUser.Role = Role.AllRoles[Random.Shared.Next(Role.AllRoles.Count)];

        gamePerspectiveService.AddUserToGame(gameId, gameUser);

        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok($"{user.DisplayName} added to game: {gameId}");
    }

    public async Task<Result<string>> RemoveUserFromGame(string gameId, string userId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        gamePerspectiveService.RemoveUserFromGame(gameId, userId);

        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok($"{user.DisplayName} removed from game: {gameId}");
    }

    public async Task<Result<string[]>> RandomiseSeatingPositions(string gameId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string[]>(Errors.GameNotFound(gameId));

        var shuffledPlayers = gamePerspective.Players.OrderBy(_ => Guid.NewGuid()).ToArray();

        foreach (GameUser shuffledPlayer in shuffledPlayers)
        {
            gamePerspectiveService.UpdatePublicUser(
                gameId,
                shuffledPlayer.Id,
                new PublicGameUserUpdate
                {
                    SeatingPosition = Array.IndexOf(shuffledPlayers, shuffledPlayer)
                }
            );
        }

        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok(shuffledPlayers.Select(u => u.Id).ToArray());
    }

    public async Task<Result<string>> SwapSeatingPositions(string gameId, string userId1, string userId2)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null)
            return Result.Fail<string>(Errors.GameNotFound(gameId));

        var user1 = gamePerspective.Users.FirstOrDefault(u => u.Id == userId1);
        if (user1 is null) return Result.Fail<string>(Errors.UserNotFound(userId1));

        var user2 = gamePerspective.Users.FirstOrDefault(u => u.Id == userId2);
        if (user2 is null) return Result.Fail<string>(Errors.UserNotFound(userId2));

        var tempPosition = user1.SeatingPosition;
        gamePerspectiveService.UpdatePublicUser(gameId, userId1, new PublicGameUserUpdate
        {
            SeatingPosition = user2.SeatingPosition
        });
        gamePerspectiveService.UpdatePublicUser(gameId, userId2, new PublicGameUserUpdate
        {
            SeatingPosition = tempPosition
        });
        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok("Users swapped");
    }

    public async Task<Result<string>> SetPlayerIsDead(string gameId, string userId, bool isDead)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        var updateOccurred = gamePerspectiveService.UpdatePublicUser(gameId, userId, new PublicGameUserUpdate
        {
            IsDead = isDead,
            HasVoteToken = isDead ? true : null
        });

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        string updateOccurredString = updateOccurred ? "now" : "already";
        string expectedTokenStatus = isDead ? "dead" : "alive";
        return Result.Ok($"{user.DisplayName} is {updateOccurredString} {expectedTokenStatus}");
    }

    public async Task<Result<string>> SetPlayerHasVoteToken(string gameId, string userId, bool hasVoteToken)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        var updateOccurred = gamePerspectiveService.UpdatePublicUser(gameId, userId, new PublicGameUserUpdate
        {
            HasVoteToken = hasVoteToken
        });

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        string updateOccurredString = updateOccurred ? "now" : "already";
        string expectedTokenStatus = hasVoteToken ? "has token" : "does not have token";
        return Result.Ok($"{user.DisplayName} {updateOccurredString} {expectedTokenStatus}");
    }
}