using Clocktower.Server.Common.Services;
using Clocktower.Server.Data.Extensions;
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
    ITownOccupancyStore townOccupancyStore,
    IJwtWriter jwtWriter,
    IMemoryCache cache,
    IOptions<Secrets> secretsOptions,
    IDiscordConstantsService discordConstants,
    IIdGenerator idGenerator) : IDiscordTownService
{
    private readonly Secrets _secrets = secretsOptions.Value;
    private const string GuildNotFoundMessage = "Guild not found";

    public async Task<(bool success, string message)> MoveUser(ulong guildId, ulong userId, ulong channelId)
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

    public (bool success, bool exists, string message) GetTownStatus(ulong guildId)
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

    public async Task<(bool success, string message)> DeleteTown(ulong guildId)
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

    public async Task<(bool success, string message)> CreateTown(ulong guildId)
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

    public async Task<(bool success, string message)> ToggleStoryTeller(string gameId, ulong userId)
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

    public async Task<(bool success, TownOccupants? townOccupants, string message)> GetTownOccupancy(ulong guildId)
    {
        try
        {
            var thisTownOccupancy = townOccupancyStore.Get(guildId);
            if (thisTownOccupancy != null) return (true, thisTownOccupancy, "Got from store");

            var guild = bot.GetGuild(guildId);
            if (guild is null) return (false, null, GuildNotFoundMessage);

            var channelCategories = new List<MiniCategory>();
            var dayCategory = guild.GetMiniCategory(discordConstants.DayCategoryName);
            if (dayCategory != null) channelCategories.Add(dayCategory);
            var nightCategory = guild.GetMiniCategory(discordConstants.NightCategoryName);
            if (nightCategory != null) channelCategories.Add(nightCategory);

            var townOccupants = new TownOccupants(channelCategories);

            townOccupancyStore.Set(guildId, townOccupants);

            var gameState = gameStateStore.GetGuildGames(guildId).FirstOrDefault();
            if (gameState is not null) await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, townOccupants);
            return (true, townOccupants, $"Town occupancy {townOccupants.UserCount}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public JoinData? GetJoinData(string key)
    {
        if (cache.TryGetValue($"join_data_{key}", out var joinData) && joinData is JoinData response)
        {
            gameStateStore.UpdateUser(response.GameId, ulong.Parse(response.User.Id), isPlaying: true);
            cache.Remove($"join_data_{key}");
            return response;
        }

        return null;
    }

    public async Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime)
    {
        try
        {
            var gameState = gameStateStore.Get(gameId);
            if (gameState is null) return (false, "Game not found");

            gameStateStore.SetTime(gameId, gameTime);
            await notificationService.BroadcastTownTime(gameId, gameTime);
            return (true, $"Time set to {gameTime}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task PingUser(string userId)
    {
        await notificationService.PingUser(userId, "Ping!");
    }

    public async Task<(InviteUserOutcome outcome, string message)> InviteUser(string gameId, ulong userId)
    {
        try
        {
            var gameState = gameStateStore.Get(gameId);
            if (gameState is null) return (InviteUserOutcome.GameDoesNotExistError, $"Couldn't find game with id: {gameId}");
            if (!ulong.TryParse(gameState.GuildId, out var guildId)) return (InviteUserOutcome.InvalidGuildError, "GameState contained a guildId that could not be found");
            var guild = bot.GetGuild(guildId);
            if (guild is null) return (InviteUserOutcome.InvalidGuildError, "GameState contained a guildId that could not be found");
            var user = guild.GetUser(userId);
            if (user is null) return (InviteUserOutcome.UserNotFoundError, $"Couldn't find user: {userId}");
            var dmChannel = await user.CreateDmChannelAsync();
            if (dmChannel is null) return (InviteUserOutcome.DmChannelError, "Couldn't open dm channel with user");

            var thisGameUser = user.AsGameUser(gameState);
            thisGameUser.UserType = UserType.Player;
            var jwt = jwtWriter.GetJwtToken(thisGameUser);
            var response = new JoinData(guildId.ToString(), thisGameUser, gameId, jwt);
            var tempKey = idGenerator.GenerateId();
            cache.Set($"join_data_{tempKey}", response, TimeSpan.FromMinutes(5));
            var url = _secrets.FeUri + $"/join?key={tempKey}";

            await dmChannel.SendMessageAsync($"[Join here]({url})");
            
            gameStateStore.AddUserToGame(gameId,thisGameUser);
            return (InviteUserOutcome.InviteSent, "Sent message to user");
        }
        catch (Exception)
        {
            return (InviteUserOutcome.UnknownError, "Failed to send message to user");
        }
    }

    private ((bool success, string message) status, (IDiscordRole role, IDiscordGuildUser user) data) ValidateToggleStoryTellerRequest(string gameId, ulong userId)
    {
        var gameState = gameStateStore.Get(gameId);
        if (gameState is null) return ((false, $"Couldn't find game with id: {gameId}"), default);

        if (!ulong.TryParse(gameState.GuildId, out var guildId)) return ((false, "GameState contained a guildId that is not valid"), default);

        var guild = bot.GetGuild(guildId);
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

    private async Task<(bool success, string message)> RemoveStoryTellerRole(string gameId, IDiscordGuildUser user, IDiscordRole role)
    {
        await user.RemoveRoleAsync(role);
        gameStateStore.UpdateUser(gameId, user.Id, UserType.Spectator);
        return (true, $"User {user.DisplayName} is no longer a {discordConstants.StoryTellerRoleName}");
    }

    private async Task<(bool success, string message)> AddStoryTellerRole(string gameId, IDiscordGuildUser user, IDiscordRole role)
    {
        await user.AddRoleAsync(role);
        gameStateStore.UpdateUser(gameId, user.Id, UserType.StoryTeller);
        return (true, $"User {user.DisplayName} is now a {discordConstants.StoryTellerRoleName}");
    }

    private ((bool success, string message) status, (IDiscordGuildUser user, IDiscordVoiceChannel channel) data) ValidateMoveUserRequest(ulong guildId, ulong userId, ulong channelId)
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