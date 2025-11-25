using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Data.Extensions;

public static class DiscordGuildExtensions
{
    extension(IDiscordGuild guild)
    {
        public MiniCategory? GetMiniCategory(string categoryName)
        {
            var categoryChannel = guild.GetCategoryChannelByName(categoryName);
            if (categoryChannel == null) return null;
            var channelOccupancy = GetChannelOccupancy(guild, categoryChannel);


            var miniCategory = new MiniCategory(categoryChannel.Id.ToString(), categoryChannel.Name, channelOccupancy);
            return miniCategory;
        }

        public IEnumerable<ChannelOccupants> GetChannelOccupancy(IDiscordCategoryChannel categoryChannel)
        {
            var channels = guild.VoiceChannels.Where(o => o.CategoryId == categoryChannel.Id)
                .OrderBy(o => o.Position);

            return (from discordChannel in channels
                let miniChannel = new MiniChannel(discordChannel.Id.ToString(), discordChannel.Name)
                let occupants = discordChannel.ConnectedUsers.Select(discordChannelUser => discordChannelUser.AsGameUser()).ToList()
                select new ChannelOccupants(miniChannel, occupants)).ToList();
        }

        public IDiscordCategoryChannel? GetCategoryChannelByName(string name)
        {
            return guild.CategoryChannels.FirstOrDefault(o => o.Name == name);
        }

        public IDiscordRole? GetRole(string roleName)
        {
            return guild.Roles.FirstOrDefault(o => o.Name == roleName);
        }
    }

    public static GameUser AsGameUser(this IDiscordGuildUser user, GameState? gameState = null)
    {
        var result = new GameUser(user.Id.ToString(), user.DisplayName, user.DisplayAvatarUrl);
        if (gameState is not null)
        {
            result.UserType = gameState.GetUserType(user.Id.ToString());
        }

        return result;
    }

    public static GameUser AsGameUser(this IDiscordUser user)
    {
        return new GameUser(user.Id.ToString(), user.GlobalName, user.DisplayAvatarUrl);
    }

    public static GameUser AsGameUser(this DiscordUser user)
    {
        var avatarUrl = user.Avatar != null
            ? $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        return new GameUser(user.Id, user.Username, avatarUrl);
    }
}