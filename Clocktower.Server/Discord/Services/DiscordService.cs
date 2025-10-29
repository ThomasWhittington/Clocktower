using DSharpPlus;
using DSharpPlus.Entities;

namespace Clocktower.Server.Discord.Services;

[UsedImplicitly]
public class DiscordService(DiscordBotService bot)
{
    private const string TownSquareName = "⛲ Town Square";
    private const string ConsultationName = "📖 Storyteller's Consultation";
    private const string DayCategoryName = "🌞 Day BOTC";
    private const string NightCategoryName = "🌙 Night BOTC ✨";
    private const string CottageName = "🛌 Cottage";
    private const string StoryTellerRoleName = "StoryTeller";

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

    public async Task<(bool success, string message)> DeleteTown(ulong guildId)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);
            var categoryChannels = (await guild.GetChannelsAsync()).Where(o => o.IsCategory).ToList();
            var dayCategory = categoryChannels.FirstOrDefault(o => o.Name == DayCategoryName);
            if (dayCategory != null) await DeleteCategory(dayCategory);
            var nightCategory = categoryChannels.FirstOrDefault(o => o.Name == NightCategoryName);
            if (nightCategory != null) await DeleteCategory(nightCategory);
            await DeleteRoles(guild);

            return (true, "Town deleted");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message )> CreateTown(ulong guildId)
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

    private static async Task<bool> DeleteRoles(DiscordGuild guild)
    {
        try
        {
            var role = guild.Roles.FirstOrDefault(o => o.Value.Name == StoryTellerRoleName).Value;
            if (role != null)
            {
                await role.DeleteAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            throw;
            return false;
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

            await CreateVoiceChannel(guild, category, TownSquareName);
            foreach (var dayRoomName in _dayRoomNames)
            {
                var success = await CreateVoiceChannel(guild, category, dayRoomName);
                if (!success) return success;
            }

            await CreateVoiceChannel(guild, category, ConsultationName);
            return true;
        }
        catch (Exception)
        {
            return false;
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


            for (int i = 0; i < 15; i++)
            {
                var result = await CreateVoiceChannel(guild, category, CottageName);
                if (!result) return result;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static async Task DeleteCategory(DiscordChannel categoryChannel)
    {
        if (categoryChannel is null) return;

        foreach (var channel in categoryChannel.Children)
        {
            await channel.DeleteAsync();
        }

        await categoryChannel.DeleteAsync();
    }

    private static async Task<bool> CreateVoiceChannel(DiscordGuild guild, DiscordChannel category, string channelName)
    {
        var result = await guild.CreateVoiceChannelAsync(channelName, parent: category);
        return result != null;
    }
}