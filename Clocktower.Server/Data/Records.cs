
using DSharpPlus.Entities;

namespace Clocktower.Server.Data;

public record MiniGuild(string Id, string Name);
[UsedImplicitly]
public record ChannelOccupants(MiniChannel Channel, IEnumerable<MiniUser> Occupants);


[UsedImplicitly]
public record MiniChannel(string Id, string Name);

[UsedImplicitly]
public record MiniCategory(string Id, string Name, IEnumerable<ChannelOccupants> Channels);

[UsedImplicitly]
public record MiniUser(string Id, string Name, string? AvatarUrl);

public class TownOccupants(List<MiniCategory> channelCategories)
{
    public int UserCount => ChannelCategories.Sum(category => category.Channels.Sum(channel => channel.Occupants.Count()));
    public List<MiniCategory> ChannelCategories { get; private set; } = channelCategories;

    public void MoveUser(DiscordUser user, DiscordVoiceState? newChannel)
    {
        var miniUser = new MiniUser(user.Id.ToString(), user.Username, user.AvatarUrl);

        ChannelCategories = ChannelCategories.Select(category =>
            category with
            {
                Channels = category.Channels.Select(channel =>
                {
                    var occupantsList = channel.Occupants
                        .Where(o => o.Id != user.Id.ToString())
                        .ToList();

                    if (newChannel?.Channel?.Id.ToString() == channel.Channel.Id)
                    {
                        occupantsList.Add(miniUser);
                    }

                    return channel with { Occupants = occupantsList };
                }).ToList()
            }).ToList();
    }
}