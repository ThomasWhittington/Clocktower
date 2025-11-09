using Discord.WebSocket;
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


        var miniCategory = new MiniCategory(categoryChannel.Id.ToString(), categoryChannel.Name, channelOccupancy);
        return miniCategory;
    }

    public static MiniCategory? GetMiniCategory(this SocketGuild guild, string categoryName)
    {
        var categoryChannel = guild.GetCategoryChannelByName(categoryName);
        if (categoryChannel == null) return null;
        var channelOccupancy = GetChannelOccupancy(guild, categoryChannel);


        var miniCategory = new MiniCategory(categoryChannel.Id.ToString(), categoryChannel.Name, channelOccupancy);
        return miniCategory;
    }

    public static IEnumerable<ChannelOccupants> GetChannelOccupancy(this SocketGuild guild, SocketCategoryChannel categoryChannel)
    {
        var channels = guild.VoiceChannels.Where(o => o.CategoryId == categoryChannel.Id);
     
        return (from discordChannel in channels
            let miniChannel = new MiniChannel(discordChannel.Id.ToString(), discordChannel.Name)
            let occupants = discordChannel.ConnectedUsers.Select(discordChannelUser =>
                new MiniUser(discordChannelUser.Id.ToString(), discordChannelUser.DisplayName, discordChannelUser.GetDisplayAvatarUrl())
            ).ToList()
            select new ChannelOccupants(miniChannel, occupants)).ToList();
    }

    public static async Task<IEnumerable<ChannelOccupants>> GetChannelOccupancy(this DiscordGuild guild, DiscordChannel categoryChannel)
    {
        var channels = await guild.GetChannels(parent: categoryChannel);

        return (from discordChannel in channels
            let miniChannel = new MiniChannel(discordChannel.Id.ToString(), discordChannel.Name)
            let occupants = discordChannel.Users.Select(discordChannelUser =>
                new MiniUser(discordChannelUser.Id.ToString(), discordChannelUser.DisplayName, discordChannelUser.AvatarUrl)
            ).ToList()
            select new ChannelOccupants(miniChannel, occupants)).ToList();
    }

    public static async Task<bool> CreateVoiceChannel(this DiscordGuild guild, DiscordChannel category, string channelName)
    {
        var result = await guild.CreateVoiceChannelAsync(channelName, parent: category);
        return result != null;
    }

    public static SocketCategoryChannel? GetCategoryChannelByName(this SocketGuild guild, string name)
    {
        return guild.CategoryChannels.FirstOrDefault(o => o.Name == name);
    }

    public static bool VerifyCategoryChannels(this SocketCategoryChannel categoryChannel, string[] channelNames)
    {
        if (categoryChannel.Channels.Count != channelNames.Length) return false;
        return channelNames.All(channelName => categoryChannel.Channels.Any(o => o.Name == channelName));
    }

    public static SocketRole? GetRole(this SocketGuild guild, string roleName)
    {
        return guild.Roles.FirstOrDefault(o => o.Name == roleName);
    }

    public static SocketUser? GetUser(this SocketGuild guild, ulong userId)
    {
        return guild.Users.FirstOrDefault(o => o.Id == userId);
    }

    public static async Task DeleteCategoryAsync(this SocketCategoryChannel categoryChannel)
    {
        if (categoryChannel is null) return;

        foreach (var channel in categoryChannel.Channels)
        {
            await channel.DeleteAsync();
        }

        await categoryChannel.DeleteAsync();
    }
}