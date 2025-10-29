using DSharpPlus;
using DSharpPlus.Entities;

namespace Clocktower.Server.Discord.Services;

[UsedImplicitly]
public class DiscordService(DiscordBotService bot)
{
    private const string TownSquareName = "⛲ Town Square";
    private const string ConsultationName = "📖 Storyteller's Consultation";

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

    public async Task<(bool success, string message)> CreateChannel(ulong guildId, string name)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);
            if (guild == null) return (false, "Guild not found");
            var result = await guild.CreateVoiceChannelAsync(name);
            if (result == null) return (false, "Channel failed to be created");
            return (true, "Channel created");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string message)> DeleteTown(ulong guildId)
    {
        try
        {
            var guild = await bot.Client.GetGuildAsync(guildId);
            var categoryChannels = (await guild.GetChannelsAsync()).Where(o => o.IsCategory).ToList();
            var dayCategory = categoryChannels.FirstOrDefault(o => o.Name.Contains("Day"));
            if (dayCategory != null) await DeleteCategory(dayCategory);
            var nightCategory = categoryChannels.FirstOrDefault(o => o.Name.Contains("Night"));
            if (nightCategory != null) await DeleteCategory(nightCategory);
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

    private async Task<bool> CreateDayVoiceChannels(DiscordGuild guild)
    {
        try
        {
            var overwrites = new List<DiscordOverwriteBuilder>
            {
                new DiscordOverwriteBuilder(guild.EveryoneRole).Allow(Permissions.AccessChannels)
            };
            var category = await guild.CreateChannelCategoryAsync("🌞 Day BOTC", overwrites: overwrites);

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

    private async Task<bool> CreateNightVoiceChannels(DiscordGuild guild)
    {
        try
        {
            var storytellerRole = guild.Roles.FirstOrDefault(r => r.Value.Name == "StoryTeller").Value;
            if (storytellerRole == null) return false;

            var overwrites = new List<DiscordOverwriteBuilder>
            {
                new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels),
                new DiscordOverwriteBuilder(storytellerRole).Allow(Permissions.AccessChannels)
            };
            var category = await guild.CreateChannelCategoryAsync("🌙 Night BOTC ✨", overwrites: overwrites);


            for (int i = 0; i < 15; i++)
            {
                var result = await CreateVoiceChannel(guild, category, "🛌 Cottage");
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

    private async Task<bool> CreateVoiceChannel(DiscordGuild guild, DiscordChannel category, string channelName)
    {
        var result = await guild.CreateVoiceChannelAsync(channelName, parent: category);
        return result != null;
    }
}