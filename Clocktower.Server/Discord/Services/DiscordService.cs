using System.Collections.Concurrent;
using Clocktower.Server.Socket;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.AspNetCore.SignalR;

namespace Clocktower.Server.Discord.Services;

[UsedImplicitly]
public class DiscordService(DiscordBotService bot, IHubContext<DiscordNotificationHub, IDiscordNotificationClient> hubContext)
{
    private const string TownSquareName = "⛲ Town Square";
    private const string ConsultationName = "📖 Storyteller's Consultation";
    private const string DayCategoryName = "🌞 Day BOTC";
    private const string NightCategoryName = "🌙 Night BOTC ✨";
    private const string CottageName = "🛌 Cottage";
    private const string StoryTellerRoleName = "StoryTeller";
    private const int CottageCount = 15;

    private readonly string[] _dayRoomNames =
    [
        "🍻 Inn",
        "🏫 School",
        "⛪ Church",
        "🔱 Devil's Lair",
        "🌳 Forbidden Forest",
        "🏰 Lost Castle",
        "🗡 Village Smithy",
        "🕍 Sacred Temple",
        "💀 Haunted Cemetery"
    ];

    private readonly ConcurrentDictionary<ulong, TownOccupants> _townOccupants = new();

    public void Initialize()
    {
        bot.Client.VoiceStateUpdated += async (_, args) =>
        {
            if (args.Before?.Channel?.Id != args.After?.Channel?.Id)
            {
                await HandleUserMoved(args.User, args.Before, args.After);
            }
        };
    }

    private async Task HandleUserMoved(DiscordUser user, DiscordVoiceState? before, DiscordVoiceState? after)
    {
        ulong guildId;
        if (before != null) guildId = before.Guild.Id;
        else if (after != null) guildId = after.Guild.Id;
        else return;

        if (_townOccupants[guildId] == null) await GetTownOccupancy(guildId);
        var thisTownOccupancy = _townOccupants[guildId];
        thisTownOccupancy!.MoveUser(user, after);
        await hubContext.Clients.All.TownOccupancyUpdated(thisTownOccupancy);
    }

    public async Task<(bool success, string message)> MoveUser(ulong guildId, ulong userId, ulong channelId)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);
            if (guild == null)
                return (false, "Guild not found");

            var channel = guild.GetChannel(channelId);
            if (channel == null)
                return (false, "Channel not found in guild");

            if (channel.Type != ChannelType.Voice)
                return (false, "Channel is not a voice channel");

            var member = await guild.GetMemberAsync(userId);
            if (member == null)
                return (false, "User not found in guild");

            if (member.VoiceState == null)
                return (false, "User is not connected to voice");

            await member.ModifyAsync(x => x.VoiceChannel = channel);
            return (true, $"User {member.DisplayName} moved to {channel.Name}");
        }
        catch (BadRequestException badRequestException)
        {
            return (false, badRequestException.JsonMessage);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, bool valid, string guildName, string message)> CheckGuildId(ulong guildId)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);
            if (guild != null)
            {
                return (true, true, guild.Name, "Bot has access to guild");
            }

            return (false, false, string.Empty, "Bot does not have access to guild");
        }
        catch (Exception)
        {
            return (false, false, string.Empty, $"Bot does not have access to guild: {guildId}");
        }
    }

    public async Task<(bool success, string message)> RebuildTown(ulong guildId)
    {
        try
        {
            var guildValid = await CheckGuildId(guildId);
            if (guildValid is not { success: true, valid: true }) return (false, guildValid.message);
            var delete = await DeleteTown(guildId);
            if (!delete.success) return delete;
            var create = await CreateTown(guildId);
            return create;
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, bool exists, string message)> TownExists(ulong guildId)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);

            var role = guild.Roles.FirstOrDefault(o => o.Value.Name == StoryTellerRoleName).Value;
            if (role == null) return (true, false, $"{StoryTellerRoleName} role does not exist. Recommend rebuild");

            var dayCategory = await guild.GetCategory(name: DayCategoryName);
            if (dayCategory == null) return (true, false, "Missing day category, Recommend rebuild");

            if (_dayRoomNames.Select(dayRoomName => dayCategory.Children.Any(o => o.Name == dayRoomName)).Any(channelExists => !channelExists))
            {
                return (true, false, "Missing day channels. Recommend rebuild");
            }

            var nightCategory = await guild.GetCategory(name: NightCategoryName);
            if (nightCategory == null) return (true, false, "Missing night category, Recommend rebuild");

            if (nightCategory.Children.Count < CottageCount)
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
            var guild = await bot.Client.GetGuildAsync(guildId);
            var dayCategory = await guild.GetCategory(name: DayCategoryName);
            if (dayCategory != null) await dayCategory.DeleteCategoryAsync();
            var nightCategory = await guild.GetCategory(name: NightCategoryName);
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
            var guild = await bot.Client.GetGuildAsync(guildId);
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
            var guild = await bot.Client.GetGuildAsync(guildId);
            var role = guild.Roles.FirstOrDefault(o => o.Value.Name == StoryTellerRoleName).Value;
            if (role == null) return (false, $"{StoryTellerRoleName} role does not exist");
            var user = await guild.GetMemberAsync(userId);
            if (user == null) return (false, "User not found");

            if (user.Roles.Contains(role))
            {
                await user.RevokeRoleAsync(role);
                return (true, $"User {user.DisplayName} is no longer a {StoryTellerRoleName}");
            }
            else
            {
                await user.GrantRoleAsync(role);
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
            var gotInCache = _townOccupants.TryGetValue(guildId, out TownOccupants? thisTownOccupancy);
            if (gotInCache && thisTownOccupancy != null) return (true, thisTownOccupancy, "Got from cache");
            var guild = await bot.Client.GetGuildAsync(guildId);

            var channelCategories = new List<MiniCategory>();
            var dayCategory = await guild.GetMiniCategory(DayCategoryName);
            if (dayCategory != null) channelCategories.Add(dayCategory);
            var nightCategory = await guild.GetMiniCategory(NightCategoryName);
            if (nightCategory != null) channelCategories.Add(nightCategory);

            var townOccupants = new TownOccupants(channelCategories);

            _townOccupants.TryAdd(guildId, townOccupants);
            return (true, townOccupants, $"Town occupancy {townOccupants.UserCount}");
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }


    private static async Task<bool> CreateNightVoiceChannels(DiscordGuild guild)
    {
        try
        {
            var storytellerRole = guild.Roles.FirstOrDefault(r => r.Value.Name == StoryTellerRoleName).Value;
            if (storytellerRole == null) return false;

            var overwrites = new List<DiscordOverwriteBuilder>
            {
                new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels),
                new DiscordOverwriteBuilder(storytellerRole).Allow(Permissions.AccessChannels)
            };
            var category = await guild.CreateChannelCategoryAsync(NightCategoryName, overwrites: overwrites);


            for (int i = 0; i < CottageCount; i++)
            {
                var result = await guild.CreateVoiceChannel(category, CottageName);
                if (!result) return result;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }


    private static async Task<bool> CreateRoles(DiscordGuild guild)
    {
        try
        {
            if (guild.Roles.Any(o => o.Value.Name != StoryTellerRoleName))
            {
                var role = await guild.CreateRoleAsync(StoryTellerRoleName, color: DiscordColor.Goldenrod);
                if (role == null) return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task DeleteRoles(DiscordGuild guild)
    {
        var role = guild.Roles.FirstOrDefault(o => o.Value.Name == StoryTellerRoleName).Value;
        if (role != null)
        {
            await role.DeleteAsync();
        }
    }

    private async Task<bool> CreateDayVoiceChannels(DiscordGuild guild)
    {
        try
        {
            var overwrites = new List<DiscordOverwriteBuilder>
            {
                new DiscordOverwriteBuilder(guild.EveryoneRole).Allow(Permissions.AccessChannels)
            };
            var category = await guild.CreateChannelCategoryAsync(DayCategoryName, overwrites: overwrites);

            await guild.CreateVoiceChannel(category, TownSquareName);
            foreach (var dayRoomName in _dayRoomNames)
            {
                var success = await guild.CreateVoiceChannel(category, dayRoomName);
                if (!success) return success;
            }

            await guild.CreateVoiceChannel(category, ConsultationName);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

[UsedImplicitly]
public record MiniChannel(string Id, string Name);

[UsedImplicitly]
public record MiniCategory(string Id, string Name, IEnumerable<ChannelOccupants> Channels);

[UsedImplicitly]
public record MiniUser(string Id, string Name);

public class TownOccupants(List<MiniCategory> channelCategories)
{
    public int UserCount => ChannelCategories.Sum(category => category.Channels.Sum(channel => channel.Occupants.Count()));
    public List<MiniCategory> ChannelCategories { get; private set; } = channelCategories;

    public void MoveUser(DiscordUser user, DiscordVoiceState? newChannel)
    {
        var miniUser = new MiniUser(user.Id.ToString(), user.Username);

        ChannelCategories = ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                {
                    var occupantsList = channel.Occupants
                        .Where(o => o.Id != user.Id.ToString())
                        .ToList();

                    if (newChannel?.Channel?.Id.ToString() == channel.Channel.Id)
                    {
                        occupantsList.Add(miniUser);
                    }

                    return channel with { Occupants = occupantsList };
                }).ToList()
            }).ToList();
    }
}

[UsedImplicitly]
public record ChannelOccupants(MiniChannel Channel, IEnumerable<MiniUser> Occupants);