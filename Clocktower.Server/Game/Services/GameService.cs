using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Game.Services;

public class GameService(IDiscordBot bot, IGamePerspectiveService gamePerspectiveService, IDiscordTownManager discordTownManager, IGameBroadcastService gameBroadcastService, IScriptProvider scriptProvider, IIdGenerator idGenerator) : IGameService
{
    private const string Now = "now";
    private const string Already = "already";
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

    public (bool success, GamePerspective? gamePerspective, string message) StartNewGame(string guildId, string userId)
    {
        var gameId = idGenerator.GenerateId();
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
            var audioEvent = gameTime switch
            {
                GameTime.Day => AudioEvent.TimeToDay,
                GameTime.Evening => AudioEvent.TimeToEvening,
                GameTime.Night => AudioEvent.TimeToNight,
                _ => AudioEvent.Stop
            };
            await gameBroadcastService.BroadcastPlayAudio(gameId, audioEvent);
            return (true, $"Time set to {gameTime}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<Result<Script>> SetScript(string gameId, ScriptSelect scriptSelect, string? json)
    {
        var gamePerspective = gamePerspectiveService.GameExists(gameId);
        if (!gamePerspective) return Result.Fail<Script>(Errors.GameNotFound(gameId));

        var scriptResult = await scriptProvider.GetScriptAsync(scriptSelect, json);
        if (scriptResult is { IsSuccess: false }) return scriptResult;
        var script = scriptResult.Value;
        if (script is null) return Result.Fail<Script>(ErrorKind.Invalid, "script.empty", "Script is empty or invalid");

        gamePerspectiveService.SetScript(gameId, script);
        await gameBroadcastService.BroadcastScriptUpdate(gameId, script);
        return Result.Ok(script);
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
        string updateOccurredString = updateOccurred ? Now : Already;
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
        string updateOccurredString = updateOccurred ? Now : Already;
        string expectedTokenStatus = hasVoteToken ? "has token" : "does not have token";
        return Result.Ok($"{user.DisplayName} {updateOccurredString} {expectedTokenStatus}");
    }

    public async Task<Result<string>> SetPerspectiveRole(string gameId, string userId, string targetUserId, string? roleId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));
        var targetUser = guild.GetUser(targetUserId);
        if (targetUser is null) return Result.Fail<string>(Errors.UserNotFound(targetUserId));
        var role = Role.AllRoles.FirstOrDefault(o => o.Id == roleId);
        if (roleId is not null && role is null) return Result.Fail<string>(ErrorKind.NotFound, "role.not_found", $"Role '{roleId}' was not found");

        var updateOccurred = gamePerspectiveService.SetRoleOnPerspective(gameId, userId, targetUserId, role);

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);

        string updateOccurredString = updateOccurred ? Now : Already;
        string roleString = role == null ? "NONE" : role.Name;
        return Result.Ok($"{targetUser.DisplayName} {updateOccurredString} has the perspective role: {roleString}");
    }

    public async Task<Result<string>> SetRole(string gameId, string targetUserId, string? roleId)
    {
        var gamePerspective = gamePerspectiveService.GetPerspective(gameId, IGamePerspectiveStore.OmniscientKey);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var targetUser = guild.GetUser(targetUserId);
        if (targetUser is null) return Result.Fail<string>(Errors.UserNotFound(targetUserId));
        var role = Role.AllRoles.FirstOrDefault(o => o.Id == roleId);
        if (roleId is not null && role is null) return Result.Fail<string>(ErrorKind.NotFound, "role.not_found", $"Role '{roleId}' was not found");

        bool updateOccurred;
        var userIsCurrentlyTraveller = gamePerspective.Users
            .FirstOrDefault(o => o.Id == targetUserId)?.Role?.Type == RoleType.Traveller;
        if (role is { Type: RoleType.Traveller })
        {
            updateOccurred = gamePerspectiveService.SetRoleOnAllPerspectives(gameId, targetUserId, role);
        }
        else if (userIsCurrentlyTraveller)
        {
            gamePerspectiveService.SetRoleOnAllPerspectives(gameId, targetUserId, null);

            updateOccurred = role == null || gamePerspectiveService.UpdatePrivateUser(gameId, targetUserId, new PrivateGameUserUpdate
            {
                RemoveRole = false,
                Role = role
            });
        }
        else
        {
            updateOccurred = gamePerspectiveService.UpdatePrivateUser(gameId, targetUserId, new PrivateGameUserUpdate
            {
                RemoveRole = role is null,
                Role = role
            });
        }

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);

        string updateOccurredString = updateOccurred ? Now : Already;
        string roleString = role == null ? "NONE" : role.Name;
        return Result.Ok($"{targetUser.DisplayName} {updateOccurredString} has the role: {roleString}");
    }

    public async Task<Result<string>> SetDraftRole(string gameId, string targetUserId, string? roleId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var targetUser = guild.GetUser(targetUserId);
        if (targetUser is null) return Result.Fail<string>(Errors.UserNotFound(targetUserId));
        var role = Role.AllRoles.FirstOrDefault(o => o.Id == roleId);
        if (roleId is not null && role is null) return Result.Fail<string>(ErrorKind.NotFound, "role.not_found", $"Role '{roleId}' was not found");

        var updateOccurred = gamePerspectiveService.UpdateDraftRole(gameId, targetUserId, role);

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);

        string updateOccurredString = updateOccurred ? Now : Already;
        string roleString = role == null ? "NONE" : role.Name;
        return Result.Ok($"{targetUser.DisplayName} {updateOccurredString} has the draft role: {roleString}");
    }

    public async Task<Result<string>> SetDraftRoles(string gameId, Dictionary<string, string> playerRoles)
    {
        var validationResult = ValidateAndBuildDraftRoleUpdates(gameId, playerRoles);
        if (!validationResult.IsSuccess) return Result.Fail<string>(validationResult.Error!);

        var updateQueue = validationResult.Value!;
        int updateCount = ApplyDraftRoleUpdates(gameId, updateQueue);

        if (updateCount > 0) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);

        return Result.Ok($"{updateCount}/{updateQueue.Count} draft roles set for players");
    }

    public async Task<Result<string>> SetReminder(string gameId, string userId, string targetUserId, string reminderId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));
        var targetUser = guild.GetUser(targetUserId);
        if (targetUser is null) return Result.Fail<string>(Errors.UserNotFound(targetUserId));
        var gameUser = gamePerspective.Users.FirstOrDefault(o => o.Id == userId);
        if (gameUser is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        if (gameUser.UserType == UserType.StoryTeller) userId = IGamePerspectiveStore.OmniscientKey;

        var parts = reminderId.Split('-', 2);
        if (parts.Length != 2) return Result.Fail<string>(ErrorKind.Invalid, "reminder.invalid", "Invalid reminder ID format");

        var reminderToken = new ReminderToken(parts[0], parts[1]);

        var updateOccurred = gamePerspectiveService.AddReminderForUserOnPerspective(gameId, userId, targetUserId, reminderToken);

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);

        string updateOccurredString = updateOccurred ? "Reminders updated" : "No reminder change made";
        return Result.Ok($"{targetUser.DisplayName} {updateOccurredString}");
    }

    public async Task<Result<string>> RemoveReminder(string gameId, string userId, string targetUserId, string reminderId)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<string>(Errors.GameNotFound(gameId));
        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<string>(Errors.InvalidGuildId());
        var user = guild.GetUser(userId);
        if (user is null) return Result.Fail<string>(Errors.UserNotFound(userId));
        var targetUser = guild.GetUser(targetUserId);
        if (targetUser is null) return Result.Fail<string>(Errors.UserNotFound(targetUserId));
        var gameUser = gamePerspective.Users.FirstOrDefault(o => o.Id == userId);
        if (gameUser is null) return Result.Fail<string>(Errors.UserNotFound(userId));

        if (gameUser.UserType == UserType.StoryTeller) userId = IGamePerspectiveStore.OmniscientKey;

        var parts = reminderId.Split('-', 2);
        if (parts.Length != 2) return Result.Fail<string>(ErrorKind.Invalid, "reminder.invalid", "Invalid reminder ID format");

        var reminderToken = new ReminderToken(parts[0], parts[1]);

        var updateOccurred = gamePerspectiveService.RemoveReminderForUserOnPerspective(gameId, userId, targetUserId, reminderToken);

        if (updateOccurred) await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);

        string updateOccurredString = updateOccurred ? "Reminders updated" : "No reminder change made";
        return Result.Ok($"{targetUser.DisplayName} {updateOccurredString}");
    }

    public async Task<Result<string>> CommitDraftRoles(string gameId)
    {
        var gameExists = gamePerspectiveService.GameExists(gameId);
        if (!gameExists) return Result.Fail<string>(Errors.GameNotFound(gameId));

        gamePerspectiveService.CommitDraftRoles(gameId);

        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok($"Draft roles committed for game {gameId}");
    }

    private Result<Dictionary<IDiscordGuildUser, Role>> ValidateAndBuildDraftRoleUpdates(
        string gameId,
        Dictionary<string, string> playerRoles)
    {
        var gamePerspective = gamePerspectiveService.GetFirstPerspective(gameId);
        if (gamePerspective is null) return Result.Fail<Dictionary<IDiscordGuildUser, Role>>(Errors.GameNotFound(gameId));

        var guild = bot.GetGuild(gamePerspective.GuildId);
        if (guild is null) return Result.Fail<Dictionary<IDiscordGuildUser, Role>>(Errors.InvalidGuildId());

        var updateQueue = new Dictionary<IDiscordGuildUser, Role>();

        foreach ((string targetUserId, string roleId) in playerRoles)
        {
            var userResult = ValidateUser(guild, targetUserId);
            if (!userResult.IsSuccess) return Result.Fail<Dictionary<IDiscordGuildUser, Role>>(userResult.Error!);

            var roleResult = ValidateRole(roleId);
            if (!roleResult.IsSuccess) return Result.Fail<Dictionary<IDiscordGuildUser, Role>>(roleResult.Error!);

            updateQueue.Add(userResult.Value!, roleResult.Value!);
        }

        return Result.Ok(updateQueue);
    }

    private static Result<IDiscordGuildUser> ValidateUser(IDiscordGuild guild, string userId)
    {
        var user = guild.GetUser(userId);
        return user is null
            ? Result.Fail<IDiscordGuildUser>(Errors.UserNotFound(userId))
            : Result.Ok(user);
    }

    private static Result<Role> ValidateRole(string roleId)
    {
        var role = Role.AllRoles.FirstOrDefault(o => o.Id == roleId);
        return roleId is not null && role is null
            ? Result.Fail<Role>(ErrorKind.NotFound, "role.not_found", $"Role '{roleId}' was not found")
            : Result.Ok(role!);
    }

    private int ApplyDraftRoleUpdates(string gameId, Dictionary<IDiscordGuildUser, Role> updateQueue)
    {
        int updateCount = 0;

        foreach ((IDiscordGuildUser targetUser, Role role) in updateQueue)
        {
            var updateOccurred = gamePerspectiveService.UpdateDraftRole(gameId, targetUser.Id, role);
            if (updateOccurred) updateCount++;
        }

        return updateCount;
    }
}