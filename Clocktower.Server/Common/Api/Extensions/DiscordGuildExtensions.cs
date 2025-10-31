using Clocktower.Server.Discord.Services;
using DSharpPlus.Entities;

namespace Clocktower.Server.Common.Api.Extensions;

public static class DiscordGuildExtensions
{
    public static async Task<DiscordChannel?> GetCategory(this DiscordGuild guild, string name = "", DiscordChannel? parent = null)
    {
        return (await GetChannels(guild, name, true, parent)).FirstOrDefault();
    }

    public static async Task<IEnumerable<DiscordChannel>> GetCategories(this DiscordGuild guild, string name = "", DiscordChannel? parent = null)
    {
        return await GetChannels(guild, name, true, parent);
    }

    public static async Task<DiscordChannel?> GetChannel(this DiscordGuild guild, string name = "", bool? isCategory = null, DiscordChannel? parent = null)
    {
        return (await GetChannels(guild, name, isCategory, parent)).FirstOrDefault();
    }

    public static async Task<IEnumerable<DiscordChannel>> GetChannels(this DiscordGuild guild, string name = "", bool? isCategory = null, DiscordChannel? parent = null)
    {
        var channels = (await guild.GetChannelsAsync()).AsEnumerable();
        if (isCategory != null)
        {
            channels = channels.Where(o => o.IsCategory == isCategory);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            channels = channels.Where(o => o.Name == name);
        }

        if (parent != null)
        {
            channels = channels.Where(o => o.Parent == parent);
        }

        return channels;
    }


    public static async Task<MiniCategory?> GetMiniCategory(this DiscordGuild guild, string categoryName)
    {
        var categoryChannels = (await guild.GetChannels(categoryName, true)).ToList();
        var categoryChannel = categoryChannels.FirstOrDefault();
        if (categoryChannel == null) return null;
        var channelOccupancy = await GetChannelOccupancy(guild, categoryChannel);


        var miniCategory = new MiniCategory(categoryChannel.Id, categoryChannel.Name, channelOccupancy);
        return miniCategory;
    }

    public static async Task<IEnumerable<ChannelOccupants>> GetChannelOccupancy(this DiscordGuild guild, DiscordChannel categoryChannel)
    {
        var channels = await guild.GetChannels(parent: categoryChannel);

        return (from discordChannel in channels
            let miniChannel = new MiniChannel(discordChannel.Id, discordChannel.Name)
            let occupants = discordChannel.Users.Select(discordChannelUser =>
                new MiniUser(discordChannelUser.Id, discordChannelUser.DisplayName)
            ).ToList()
            select new ChannelOccupants(miniChannel, occupants)).ToList();
    }

    public static async Task<bool> CreateVoiceChannel(this DiscordGuild guild, DiscordChannel category, string channelName)
    {
        var result = await guild.CreateVoiceChannelAsync(channelName, parent: category);
        return result != null;
    }
}