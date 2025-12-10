namespace Clocktower.Server.Data;

public sealed record DiscordUser(
    string Id,
    string Username,
    string? Email,
    string? Avatar,
    bool? Verified,
    string Discriminator
)
{
    public TownUser AsTownUser()
    {
        var avatarUrl = Avatar != null
            ? $"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.png"
            : "https://cdn.discordapp.com/embed/avatars/0.png";

        return new TownUser(Id, Username, avatarUrl);
    }
};