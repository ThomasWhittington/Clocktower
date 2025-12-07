namespace Clocktower.Server.Data.Extensions;

public static class ToGameUserExtensions
{
    public static GameUser AsGameUser(this DiscordUser user)
    {
        var avatarUrl = user.Avatar != null
            ? $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        return new GameUser(user.Id, user.Username, avatarUrl);
    }
}