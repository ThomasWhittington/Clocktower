using Clocktower.Server.Data.Extensions;
using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Data;

public record MiniGuild(string Id, string Name);

[UsedImplicitly]
public record ChannelOccupants(MiniChannel Channel, IEnumerable<GameUser> Occupants);

[UsedImplicitly]
public record MiniChannel(string Id, string Name);

[UsedImplicitly]
public record MiniCategory(string Id, string Name, IEnumerable<ChannelOccupants> Channels);

public sealed class TokenResponse
{
    public string AccessToken { get; init; }
    public string TokenType { get; init; }
    public int ExpiresIn { get; init; }
    public string RefreshToken { get; init; }
    public string Scope { get; init; }
}

public sealed record DiscordUser(
    string Id,
    string Username,
    string? Email,
    string? Avatar,
    bool? Verified,
    string Discriminator
);

public record UserAuthData(GameUser GameUser, string Jwt);

[UsedImplicitly]
public class TownOccupants(List<MiniCategory> channelCategories)
{
    public int UserCount => ChannelCategories.Sum(category => category.Channels.Sum(channel => channel.Occupants.Count()));
    public List<MiniCategory> ChannelCategories { get; private set; } = channelCategories;

    public void MoveUser(IDiscordUser user, IDiscordVoiceChannel? newChannel)
    {
        ChannelCategories = ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                {
                    var occupantsList = channel.Occupants
                        .Where(o => o.Id != user.Id.ToString())
                        .ToList();

                    if (newChannel?.Id.ToString() == channel.Channel.Id)
                    {
                        occupantsList.Add(user.AsGameUser());
                    }

                    return channel with { Occupants = occupantsList };
                }).ToList()
            }
        ).ToList();
    }
}

[UsedImplicitly]
public record MiniGameState(string GameId, GameUser CreatedBy, DateTime CreatedDate);