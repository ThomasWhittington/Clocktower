using Clocktower.Server.Data.Extensions;
using Discord.WebSocket;

namespace Clocktower.Server.Common.Api.Extensions;

public static class DiscordGuildExtensions
{
    extension(SocketGuild guild)
    {
        public MiniCategory? GetMiniCategory(string categoryName)
        {
            var categoryChannel = guild.GetCategoryChannelByName(categoryName);
            if (categoryChannel == null) return null;
            var channelOccupancy = GetChannelOccupancy(guild, categoryChannel);


            var miniCategory = new MiniCategory(categoryChannel.Id.ToString(), categoryChannel.Name, channelOccupancy);
            return miniCategory;
        }

        public IEnumerable<ChannelOccupants> GetChannelOccupancy(SocketCategoryChannel categoryChannel)
        {
            var channels = guild.VoiceChannels.Where(o => o.CategoryId == categoryChannel.Id)
                .OrderBy(o => o.Position);

            return (from discordChannel in channels
                let miniChannel = new MiniChannel(discordChannel.Id.ToString(), discordChannel.Name)
                let occupants = discordChannel.ConnectedUsers.Select(discordChannelUser => discordChannelUser.AsGameUser()).ToList()
                select new ChannelOccupants(miniChannel, occupants)).ToList();
        }

        public SocketCategoryChannel? GetCategoryChannelByName(string name)
        {
            return guild.CategoryChannels.FirstOrDefault(o => o.Name == name);
        }

        public SocketRole? GetRole(string roleName)
        {
            return guild.Roles.FirstOrDefault(o => o.Name == roleName);
        }
    }

    public static GameUser AsGameUser(this SocketGuildUser user, GameState? gameState = null)
    {
        var result = new GameUser(user.Id.ToString(), user.DisplayName, user.GetDisplayAvatarUrl());
        if (gameState is not null)
        {
            result.UserType = gameState.GetUserType(user.Id.ToString());
        }

        return result;
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