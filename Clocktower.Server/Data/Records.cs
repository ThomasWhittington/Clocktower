using Discord.WebSocket;

namespace Clocktower.Server.Data;

public record MiniGuild(string Id, string Name);

[UsedImplicitly]
public record ChannelOccupants(MiniChannel Channel, IEnumerable<GameUser> Occupants);

[UsedImplicitly]
public record MiniChannel(string Id, string Name);

[UsedImplicitly]
public record MiniCategory(string Id, string Name, IEnumerable<ChannelOccupants> Channels);

public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string RefreshToken,
    string Scope
);

public sealed record DiscordUser(
    string Id,
    string Username,
    string? Email,
    string? Avatar,
    bool? Verified,
    string Discriminator
);

[UsedImplicitly]
public class TownOccupants(List<MiniCategory> channelCategories)
{
    public int UserCount => ChannelCategories.Sum(category => category.Channels.Sum(channel => channel.Occupants.Count()));
    public List<MiniCategory> ChannelCategories { get; private set; } = channelCategories;

    public void MoveUser(SocketUser user, SocketVoiceState? newChannel)
    {
        ChannelCategories = ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                {
                    var occupantsList = channel.Occupants
                        .Where(o => o.Id != user.Id.ToString())
                        .ToList();

                    if (newChannel?.VoiceChannel?.Id.ToString() == channel.Channel.Id)
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