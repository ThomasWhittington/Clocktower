using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Data.Extensions;

public static class ToGameUserExtensions
{

    public static GameUser AsGameUser(this IDiscordGuildUser user, GameState? gameState = null)
    {
        var result = new GameUser(user.Id.ToString(), user.DisplayName, user.DisplayAvatarUrl);
        if (gameState is not null)
        {
            result.UserType = gameState.GetUserType(user.Id.ToString());
        }

        return result;
    }

    public static GameUser AsGameUser(this DiscordUser user)
    {
        var avatarUrl = user.Avatar != null
            ? $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        return new GameUser(user.Id, user.Username, avatarUrl);
    }
}