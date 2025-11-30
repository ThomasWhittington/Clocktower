namespace Clocktower.Server.Data;

public sealed record DiscordUser(
    string Id,
    string Username,
    string? Email,
    string? Avatar,
    bool? Verified,
    string Discriminator
);