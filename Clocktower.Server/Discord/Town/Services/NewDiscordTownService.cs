using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Wrappers;
using Clocktower.Server.Socket;
using Discord;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Clocktower.Server.Discord.Town.Services;

[UsedImplicitly]
public class DiscordTownService(
    IDiscordBot bot,
    INotificationService notificationService,
    IGameStateStore gameStateStore,
    IDiscordTownStore discordTownStore,
    IJwtWriter jwtWriter,
    IMemoryCache cache,
    IOptions<Secrets> secretsOptions,
    IDiscordConstantsService discordConstants,
    IIdGenerator idGenerator) : IDiscordTownService
{
    private readonly Secrets _secrets = secretsOptions.Value;
    private const string GuildNotFoundMessage = "Guild not found";

    public async Task<(bool success, string message)> MoveUser(string guildId, string userId, string channelId)
    {
        try
        {
            var validationResult = ValidateMoveUserRequest(guildId, userId, channelId);
            if (!validationResult.status.success) return validationResult.status;
            var (user, channel) = validationResult.data;
            await user.MoveAsync(channel);
            return (true, $"User {user.DisplayName} moved to {channel.Name}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, bool exists, string message) GetTownStatus(string guildId)
    {
        try
        {
            var guild = bot.GetGuild(guildId);
            if (guild is null) return (false, false, GuildNotFoundMessage);

            var role = guild.GetRole(discordConstants.StoryTellerRoleName);
            if (role == null) return (true, false, $"{discordConstants.StoryTellerRoleName} role does not exist");

            var dayCategory = guild.GetCategoryChannelByName(name: discordConstants.DayCategoryName);
            if (dayCategory == null) return (true, false, "Missing day category");
            if (!dayCategory.VerifyCategoryChannels(discordConstants.DayRoomNames)) return (true, false, "Missing day channels");

            var nightCategory = guild.GetCategoryChannelByName(name: discordConstants.NightCategoryName);
            if (nightCategory == null) return (true, false, "Missing night category");
            if (!nightCategory.VerifyCategoryChannels(discordConstants.GetNightRoomNames())) return (true, false, "Not enough cottages");

            return (true, true, "Town structure is correct");
        }
        catch (Exception ex)
        {
            return (false, false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeleteTown(string guildId)
    {
        try
        {
            var guild = bot.GetGuild(guildId);
            if (guild is null) return (false, GuildNotFoundMessage);
            var dayCategory = guild.GetCategoryChannelByName(name: discordConstants.DayCategoryName);
            if (dayCategory != null) await dayCategory.DeleteAsync();
            var nightCategory = guild.GetCategoryChannelByName(name: discordConstants.NightCategoryName);
            if (nightCategory != null) await nightCategory.DeleteAsync();
            await guild.DeleteRoleAsync(discordConstants.StoryTellerRoleName);

            return (true, "Town deleted");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> CreateTown(string guildId)
    {
        try
        {
            var guild = bot.GetGuild(guildId);
            if (guild is null) return (false, GuildNotFoundMessage);
            var storyTellerRole = await guild.CreateRoleAsync(discordConstants.StoryTellerRoleName, Color.Gold);
            if (storyTellerRole is null) return (false, "Failed to create role");

            var dayCategory = await guild.CreateCategoryAsync(discordConstants.DayCategoryName, true);
            var dayCreated = await guild.CreateVoiceChannelsForCategoryAsync(discordConstants.DayRoomNames, dayCategory.Id);
            if (!dayCreated) return (false, "Failed to generate day channels");


            var nightCategory = await guild.CreateCategoryAsync(discordConstants.NightCategoryName, false, storyTellerRole);
            var nightCreated = await guild.CreateVoiceChannelsForCategoryAsync(discordConstants.GetNightRoomNames(), nightCategory.Id);
            if (!nightCreated)
            {
                return (false, "Failed to generate night channels");
            }

            return (true, "Town created");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> ToggleStoryTeller(string gameId, string userId)
    {
        try
        {
            var validationResult = ValidateToggleStoryTellerRequest(gameId, userId);
            if (!validationResult.status.success) return validationResult.status;
            var (role, user) = validationResult.data;

            bool isStoryTellerAlready = user.DoesUserHaveRole(role.Id);

            EnsureUserExistsInGameState(gameId, user);

            if (isStoryTellerAlready)
                return await RemoveStoryTellerRole(gameId, user, role);

            return await AddStoryTellerRole(gameId, user, role);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the DiscordTown representation for the specified guild, creating and caching it if absent, and triggers a town update notification for any active game in that guild.
    /// </summary>
    /// <param name="guildId">The ID of the Discord guild to retrieve the town for.</param>
    /// <returns>
    /// A tuple (success, discordTown, message):
    /// - `success`: `true` if the operation completed and a DiscordTown was obtained or created, `false` otherwise.
    /// - `discordTown`: the cached or newly constructed DiscordTown, or `null` if not found or on failure.
    /// - `message`: a short description of the outcome or error.
    /// </returns>
    public async Task<(bool success, DiscordTown? discordTown, string message)> GetDiscordTown(string guildId)
    {
        try
        {
            var thisDiscordTown = discordTownStore.Get(guildId);
            if (thisDiscordTown != null) return (true, thisDiscordTown, "Got from store");

            var guild = bot.GetGuild(guildId);
            if (guild is null) return (false, null, GuildNotFoundMessage);

            var channelCategories = new List<MiniCategory>();
            var dayCategory = guild.GetMiniCategory(discordConstants.DayCategoryName);
            if (dayCategory != null) channelCategories.Add(dayCategory);
            var nightCategory = guild.GetMiniCategory(discordConstants.NightCategoryName);
            if (nightCategory != null) channelCategories.Add(nightCategory);

            var discordTown = new DiscordTown(channelCategories);

            discordTownStore.Set(guildId, discordTown);

            var gameState = gameStateStore.GetGuildGames(guildId).FirstOrDefault();
            if (gameState is not null) await notificationService.BroadcastDiscordTownUpdate(gameState.Id);
            return (true, discordTown, $"Discord town {discordTown.UserCount}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, DiscordTownDto? discordTown, string message)> GetDiscordTownDto(string gameId)
    {
        var gameState = gameStateStore.Get(gameId);
        if (gameState is null) return (false, null, $"Game not found for id: {gameId}");

        var (success, discordTown, message) = await GetDiscordTown(gameState.GuildId);
        if (success && discordTown != null)
        {
            var discordTownDto = discordTown.ToDiscordTownDto(gameId, gameState.Users);
            return (true, discordTownDto, message);
        }

        return (success, null, message);
    }

    public JoinData? GetJoinData(string key)
    {
        if (cache.TryGetValue($"join_data_{key}", out var joinData) && joinData is JoinData response)
        {
            gameStateStore.UpdateUser(response.GameId, response.User.Id, isPlaying: true);
            cache.Remove($"join_data_{key}");
            return response;
        }

        return null;
    }

    public async Task PingUser(string userId)
    {
        await notificationService.PingUser(userId, "Ping!");
    }

    public async Task<(InviteUserOutcome outcome, string message)> InviteUser(string gameId, string userId)
    {
        try
        {
            var gameState = gameStateStore.Get(gameId);
            if (gameState is null) return (InviteUserOutcome.GameDoesNotExistError, $"Couldn't find game with id: {gameId}");
            var guild = bot.GetGuild(gameState.GuildId);
            if (guild is null) return (InviteUserOutcome.InvalidGuildError, "GameState contained a guildId that could not be found");

            var user = guild.GetUser(userId);
            if (user is null) return (InviteUserOutcome.UserNotFoundError, $"Couldn't find user: {userId}");
            var dmChannel = await user.CreateDmChannelAsync();
            if (dmChannel is null) return (InviteUserOutcome.DmChannelError, "Couldn't open dm channel with user");

            var thisGameUser = user.AsGameUser(gameState);
            thisGameUser.UserType = UserType.Player;
            var jwt = jwtWriter.GetJwtToken(thisGameUser);
            var response = new JoinData(guild.Id, thisGameUser, gameId, jwt);
            var tempKey = idGenerator.GenerateId();
            cache.Set($"join_data_{tempKey}", response, TimeSpan.FromMinutes(5));
            var url = _secrets.FeUri + $"/join?key={tempKey}";

            await dmChannel.SendMessageAsync($"[Join here]({url})");

            gameStateStore.AddUserToGame(gameId, thisGameUser);
            return (InviteUserOutcome.InviteSent, "Sent message to user");
        }
        catch (Exception)
        {
            return (InviteUserOutcome.UnknownError, "Failed to send message to user");
        }
    }

    /// <summary>
    /// Validates that the game, guild, storyteller role, and specified user exist for toggling StoryTeller status.
    /// </summary>
    /// <param name="gameId">The game identifier used to locate the game state and its guild.</param>
    /// <param name="userId">The Discord user identifier to validate within the guild.</param>
    /// <returns>
    /// A tuple where the first element is a status (success flag and message) and the second element is data (the StoryTeller role and the guild user).
    /// `success` is `true` when all validations pass and `message` is empty; otherwise `success` is `false` and `message` explains the failure.
    /// When `success` is `true`, `data.role` is the StoryTeller role and `data.user` is the guild user; otherwise `data` is `default`.
    /// </returns>
    private ((bool success, string message) status, (IDiscordRole role, IDiscordGuildUser user) data) ValidateToggleStoryTellerRequest(string gameId, string userId)
    {
        var gameState = gameStateStore.Get(gameId);
        if (gameState is null) return ((false, $"Couldn't find game with id: {gameId}"), default);

        var guild = bot.GetGuild(gameState.GuildId);
        if (guild is null) return ((false, GuildNotFoundMessage), default);

        var role = guild.GetRole(discordConstants.StoryTellerRoleName);
        if (role is null) return ((false, $"{discordConstants.StoryTellerRoleName} role does not exist"), default);

        var user = guild.GetUser(userId);
        if (user is null) return ((false, "User not found"), default);

        return ((true, string.Empty), (role, user));
    }


    private void EnsureUserExistsInGameState(string gameId, IDiscordGuildUser user)
    {
        var thisUser = gameStateStore.Get(gameId)?.Users.GetById(user.Id);
        if (thisUser is null) gameStateStore.AddUserToGame(gameId, user.AsGameUser());
    }

    /// <summary>
    /// Remove the StoryTeller role from a guild user, mark them as a spectator in the game state, and broadcast a town update.
    /// </summary>
    /// <param name="gameId">The identifier of the game whose state should be updated and notified.</param>
    /// <param name="user">The guild user to remove the StoryTeller role from.</param>
    /// <param name="role">The StoryTeller role to remove.</param>
    /// <returns>`true` and a confirmation message indicating the user is no longer the StoryTeller.</returns>
    private async Task<(bool success, string message)> RemoveStoryTellerRole(string gameId, IDiscordGuildUser user, IDiscordRole role)
    {
        await user.RemoveRoleAsync(role);
        gameStateStore.UpdateUser(gameId, user.Id, UserType.Spectator);
        await notificationService.BroadcastDiscordTownUpdate(gameId);
        return (true, $"User {user.DisplayName} is no longer a {discordConstants.StoryTellerRoleName}");
    }

    /// <summary>
    /// Grants the StoryTeller role to the specified guild user, updates the user's type in the game state, and broadcasts a town update.
    /// </summary>
    /// <param name="gameId">The identifier of the game whose state should be updated.</param>
    /// <param name="user">The guild user to receive the StoryTeller role.</param>
    /// <param name="role">The StoryTeller role to assign to the user.</param>
    /// <returns>`(true, message)` where `true` indicates the role was added and `message` is a confirmation containing the user's display name and role name.</returns>
    private async Task<(bool success, string message)> AddStoryTellerRole(string gameId, IDiscordGuildUser user, IDiscordRole role)
    {
        await user.AddRoleAsync(role);
        gameStateStore.UpdateUser(gameId, user.Id, UserType.StoryTeller);
        await notificationService.BroadcastDiscordTownUpdate(gameId);
        return (true, $"User {user.DisplayName} is now a {discordConstants.StoryTellerRoleName}");
    }

    private ((bool success, string message) status, (IDiscordGuildUser user, IDiscordVoiceChannel channel) data) ValidateMoveUserRequest(string guildId, string userId, string channelId)
    {
        var guild = bot.GetGuild(guildId);
        if (guild == null) return ((false, GuildNotFoundMessage), default);

        var user = guild.GetUser(userId);
        if (user == null) return ((false, "User not found in guild"), default);

        if (user.VoiceState == null) return ((false, "User is not connected to voice"), default);

        var channel = guild.GetVoiceChannel(channelId);
        if (channel == null) return ((false, "Channel not found in guild"), default);
        return ((true, string.Empty), (user, channel));
    }
}