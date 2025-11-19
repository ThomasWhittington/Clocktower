using Clocktower.Server.Common.Services;
using Clocktower.Server.Discord.Services;
using Clocktower.Server.Socket;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Clocktower.Server.Discord.Town.Services;

[UsedImplicitly]
public class DiscordTownService(
    IDiscordBotService bot,
    INotificationService notificationService,
    IGameStateStore gameStateStore,
    IJwtWriter jwtWriter,
    IMemoryCache cache,
    IOptions<Secrets> secretsOptions) : IDiscordTownService
{
    private readonly Secrets _secrets = secretsOptions.Value;
    private const string TownSquareName = "⛲ Town Square";
    private const string ConsultationName = "📖 Storyteller's Consultation";
    private const string DayCategoryName = "🌞 Day BOTC";
    private const string NightCategoryName = "🌙 Night BOTC ✨";
    private const string CottageName = "🛌 Cottage";
    private const string StoryTellerRoleName = "StoryTeller";
    private const int CottageCount = 15;

    private readonly string[] _dayRoomNames =
    [
        TownSquareName,
        "🍻 Inn",
        "🏫 School",
        "⛪ Church",
        "🔱 Devil's Lair",
        "🌳 Forbidden Forest",
        "🏰 Lost Castle",
        "🗡 Village Smithy",
        "🕍 Sacred Temple",
        "💀 Haunted Cemetery",
        ConsultationName
    ];

    public async Task<(bool success, string message)> MoveUser(ulong guildId, ulong userId, ulong channelId)
    {
        try
        {
            var guild = bot.Client.GetGuild(guildId);
            if (guild == null)
                return (false, "Guild not found");

            var channel = guild.GetVoiceChannel(channelId);
            if (channel == null)
                return (false, "Channel not found in guild");

            var member = guild.GetUser(userId);
            if (member == null)
                return (false, "User not found in guild");

            if (member.VoiceState == null)
                return (false, "User is not connected to voice");

            await guild.MoveAsync(member, channel);
            return (true, $"User {member.DisplayName} moved to {channel.Name}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> RebuildTown(ulong guildId)
    {
        try
        {
            var gameState = gameStateStore.GetGuildGames(guildId).FirstOrDefault();
            if (gameState is null) return (false, "Failed to find gameState");
            await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, new TownOccupants([]));
            var delete = await DeleteTown(guildId);
            if (!delete.success) return delete;
            var create = await CreateTown(guildId);
            if (create.success)
            {
                TownOccupancyStore.Clear();
                await GetTownOccupancy(guildId);
            }

            return create;
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public (bool success, bool exists, string message) TownExists(ulong guildId)
    {
        try
        {
            var guild = bot.Client.GetGuild(guildId);

            var role = guild.Roles.FirstOrDefault(o => o.Name == StoryTellerRoleName);
            if (role == null) return (true, false, $"{StoryTellerRoleName} role does not exist. Recommend rebuild");

            var dayCategory = guild.GetCategoryChannelByName(name: DayCategoryName);
            if (dayCategory == null) return (true, false, "Missing day category, Recommend rebuild");

            if (!dayCategory.VerifyCategoryChannels(_dayRoomNames))
            {
                return (true, false, "Missing day channels. Recommend rebuild");
            }

            var nightCategory = guild.GetCategoryChannelByName(name: NightCategoryName);
            if (nightCategory == null) return (true, false, "Missing night category, Recommend rebuild");

            if (nightCategory.Channels.Count < CottageCount)
            {
                return (true, false, "Not enough cottages, Recommend rebuild");
            }

            return (true, true, "Town exists");
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
            var guild = bot.Client.GetGuild(guildId);
            var dayCategory = guild.GetCategoryChannelByName(name: DayCategoryName);
            if (dayCategory != null) await dayCategory.DeleteCategoryAsync();
            var nightCategory = guild.GetCategoryChannelByName(name: NightCategoryName);
            if (nightCategory != null) await nightCategory.DeleteCategoryAsync();
            await DeleteRoles(guild);

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
            var guild = bot.Client.GetGuild(guildId);
            var roleCreated = await CreateRoles(guild);
            if (!roleCreated) return (false, "Failed to generate roles");
            var dayCreated = await CreateDayVoiceChannels(guild);
            if (!dayCreated) return (false, "Failed to generate day channels");
            var nightCreated = await CreateNightVoiceChannels(guild);
            if (!nightCreated) return (false, "Failed to generate night channels");
            return (true, "Town created");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> ToggleStoryTeller(ulong guildId, ulong userId)
    {
        try
        {
            var guild = bot.Client.GetGuild(guildId);
            var role = guild.GetRole(StoryTellerRoleName);
            if (role is null) return (false, $"{StoryTellerRoleName} role does not exist");
            var user = guild.GetUser(userId);
            if (user is null) return (false, "User not found");
            if (user.Roles.Any(r => r.Id == role.Id))
            {
                await user.RemoveRoleAsync(role);
                return (true, $"User {user.DisplayName} is no longer a {StoryTellerRoleName}");
            }
            else
            {
                await user.AddRoleAsync(role);
                return (true, $"User {user.DisplayName} is now a {StoryTellerRoleName}");
            }
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
            var thisTownOccupancy = TownOccupancyStore.Get(guildId);
            if (thisTownOccupancy != null) return (true, thisTownOccupancy, "Got from cache");
            var gameState = gameStateStore.GetGuildGames(guildId).FirstOrDefault();
            if (gameState is null) return (false, null, "Failed to find gameState");

            var guild = bot.Client.GetGuild(guildId);

            var channelCategories = new List<MiniCategory>();
            var dayCategory = guild.GetMiniCategory(DayCategoryName);
            if (dayCategory != null) channelCategories.Add(dayCategory);
            var nightCategory = guild.GetMiniCategory(NightCategoryName);
            if (nightCategory != null) channelCategories.Add(nightCategory);

            var townOccupants = new TownOccupants(channelCategories);

            TownOccupancyStore.Set(guildId, townOccupants);
            await notificationService.BroadcastTownOccupancyUpdate(gameState.Id, townOccupants);
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
            gameStateStore.TryUpdate(response.GameId, state =>
            {
                state.Players.First(o => o.Id == response.User.Id).IsPlaying = true;
                return state;
            });

            cache.Remove($"join_data_{key}");
            return response;
        }

        return null;
    }

    public async Task<(bool success, string message)> SetTime(string gameId, GameTime gameTime)
    {
        var gameState = gameStateStore.Get(gameId);
        if (gameState is null) return (false, "Game not found");
        var guild = bot.Client.GetGuild(ulong.Parse(gameState.GuildId));
        if (guild is null) return (false, "Guild not found");
        await notificationService.BroadcastTownTime(gameId, gameTime);
        gameStateStore.TryUpdate(gameId, state =>
        {
            state.GameTime = gameTime;
            return state;
        });
        return (true, $"Time set to {gameTime}");
    }


    private static async Task<bool> CreateNightVoiceChannels(SocketGuild guild)
    {
        try
        {
            var storytellerRole = guild.GetRole(StoryTellerRoleName);
            if (storytellerRole == null) return false;

            var category = await guild.CreateCategoryChannelAsync(NightCategoryName, properties =>
            {
                properties.PermissionOverwrites = new[]
                {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                        new OverwritePermissions(viewChannel: PermValue.Deny)
                    ),
                    new Overwrite(storytellerRole.Id, PermissionTarget.Role,
                        new OverwritePermissions(viewChannel: PermValue.Allow)
                    ),
                };
            });

            for (int i = 0; i < CottageCount; i++)
            {
                var result = await guild.CreateVoiceChannelAsync(CottageName, properties => properties.CategoryId = category.Id);
                if (result is null) return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }


    private static async Task<bool> CreateRoles(SocketGuild guild)
    {
        try
        {
            var role = guild.GetRole(StoryTellerRoleName);
            if (role is null)
            {
                var newRole = await guild.CreateRoleAsync(StoryTellerRoleName, color: Color.Gold);
                if (newRole is null) return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task DeleteRoles(SocketGuild guild)
    {
        var role = guild.GetRole(StoryTellerRoleName);
        if (role != null)
        {
            await role.DeleteAsync();
        }
    }

    private async Task<bool> CreateDayVoiceChannels(SocketGuild guild)
    {
        try
        {
            var category = await guild.CreateCategoryChannelAsync(DayCategoryName, properties =>
            {
                properties.PermissionOverwrites = new[]
                {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role,
                        new OverwritePermissions(viewChannel: PermValue.Allow)
                    )
                };
            });


            foreach (var dayRoomName in _dayRoomNames)
            {
                var result = await guild.CreateVoiceChannelAsync(dayRoomName, properties => properties.CategoryId = category.Id);
                if (result is null) return false;
            }


            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task PingUser(string userId)
    {
        await notificationService.PingUser(userId, "Ping!");
    }

    public async Task<(InviteUserOutcome outcome, string message )> InviteUser(string gameId, ulong userId)
    {
        try
        {
            var gameState = gameStateStore.Get(gameId);
            if (gameState is null) return (InviteUserOutcome.GameDoesNotExistError, $"Couldn't find game with id: {gameId}");
            if (!ulong.TryParse(gameState.GuildId, out var guildId)) return (InviteUserOutcome.InvalidGuildError, "GameState contained a guildId that could not be found");
            var guild = bot.Client.GetGuild(guildId);
            if (guild is null) return (InviteUserOutcome.InvalidGuildError, "GameState contained a guildId that could not be found");
            var user = guild.GetUser(userId);
            if (user is null) return (InviteUserOutcome.UserNotFoundError, $"Couldn't find user: {userId}");
            var thisGameUser = user.AsGameUser(gameState);
            thisGameUser.UserType = UserType.Player;
            var jwt = jwtWriter.GetJwtToken(thisGameUser);
            var response = new JoinData(guildId.ToString(), thisGameUser, gameId, jwt);
            var tempKey = Guid.NewGuid().ToString();
            cache.Set($"join_data_{tempKey}", response, TimeSpan.FromMinutes(5));
            var url = _secrets.FeUri + $"/join?key={tempKey}";

            var dmChannel = await user.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"[Join here]({url})");


            gameStateStore.TryUpdate(gameId, state =>
            {
                state.Users.Add(thisGameUser);
                return state;
            });

            return (InviteUserOutcome.InviteSent, "Sent message to user");
        }
        catch (Exception)
        {
            return (InviteUserOutcome.UnknownError, "Failed to send message to user");
        }
    }
}