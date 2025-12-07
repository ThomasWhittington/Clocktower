using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public class TownOccupantManager : ITownOccupantManager
{
    public TownOccupants MoveUser(TownOccupants current, IDiscordGuildUser user, IDiscordVoiceChannel? newChannel)
    {
        var currentChannel = FindUserChannel(current, user.Id.ToString());
        if (currentChannel is not null && newChannel is not null &&
            currentChannel.Channel.Id == newChannel.Id.ToString())
            return current;

        var newChannelCategories = current.ChannelCategories.Select(category =>
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

        return new TownOccupants(newChannelCategories);
    }

    public ChannelOccupants? FindUserChannel(TownOccupants occupants, string userId)
    {
        return occupants.ChannelCategories
            .SelectMany(category => category.Channels)
            .FirstOrDefault(channel => channel.Occupants.Any(occupant => occupant.Id == userId));
    }
}