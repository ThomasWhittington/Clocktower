using Discord.WebSocket;

namespace Clocktower.Server.Common.Api.Extensions;

public static class DiscordGuildExtensions
{
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
        var channels = guild.VoiceChannels.Where(o => o.CategoryId == categoryChannel.Id)
            .OrderBy(o => o.Position);

        return (from discordChannel in channels
            let miniChannel = new MiniChannel(discordChannel.Id.ToString(), discordChannel.Name)
            let occupants = discordChannel.ConnectedUsers.Select(discordChannelUser => discordChannelUser.AsGameUser()).ToList()
            select new ChannelOccupants(miniChannel, occupants)).ToList();
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

    public static GameUser AsGameUser(this SocketGuildUser user)
    {
        return new GameUser(user.Id.ToString(), user.DisplayName, user.GetDisplayAvatarUrl());
    }

    public static GameUser AsGameUser(this SocketUser user)
    {
        return new GameUser(user.Id.ToString(), user.GlobalName, user.GetDisplayAvatarUrl());
    }

    public static GameUser AsGameUser(this DiscordUser user)
    {
        var avatarUrl = user.Avatar != null
            ? $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

       return new GameUser(user.Id, user.Username, avatarUrl);
    }
}