using Clocktower.Server.Data.Extensions;
using Clocktower.Server.Data.Wrappers;

namespace Clocktower.Server.Common.Services;

public class TownOccupantManager : ITownOccupantManager
{
    public TownOccupants MoveUser(TownOccupants current, IDiscordUser user, IDiscordVoiceChannel? newChannel)
    {
        var userId = user.Id.ToString();
        var newChannelId = newChannel?.Id.ToString();

        var (userLocation, channelMap) = BuildLookupMaps(current);
        if (userLocation.TryGetValue(userId, out var currentChannelId) && currentChannelId == newChannelId) return current;

        var categoriesToUpdate = new HashSet<string>();
        if (currentChannelId is not null && channelMap.TryGetValue(currentChannelId, out var currentCategoryId))
        {
            categoriesToUpdate.Add(currentCategoryId);
        }

        if (newChannelId is not null && channelMap.TryGetValue(newChannelId, out var newCategoryId))
        {
            categoriesToUpdate.Add(newCategoryId);
        }

        var updatedCategories = current.ChannelCategories.Select(category =>
            categoriesToUpdate.Contains(category.Id)
                ? UpdateCategoryForUserMove(category, userId, newChannelId, user)
                : category
        ).ToList();

        return new TownOccupants(updatedCategories);
    }

    private static MiniCategory UpdateCategoryForUserMove(
        MiniCategory category,
        string userId,
        string? newChannelId,
        IDiscordUser user)
    {
        var updatedChannels = category.Channels.Select(channel =>
        {
            var channelId = channel.Channel.Id;
            var currentOccupants = channel.Occupants.ToList();

            var filteredOccupants = currentOccupants.Where(o => o.Id != userId);

            if (channelId == newChannelId)
            {
                var newOccupantsList = filteredOccupants.ToList();
                newOccupantsList.Add(user.AsGameUser());
                return channel with { Occupants = newOccupantsList };
            }

            var filteredList = filteredOccupants.ToList();
            return filteredList.Count != currentOccupants.Count
                ? channel with { Occupants = filteredList }
                : channel;
        }).ToList();

        return category with { Channels = updatedChannels };
    }

    private static (Dictionary<string, string> userLocation, Dictionary<string, string> channelMap)
        BuildLookupMaps(TownOccupants occupants)
    {
        var userLocation = new Dictionary<string, string>();
        var channelMap = new Dictionary<string, string>();

        foreach (var category in occupants.ChannelCategories)
        {
            foreach (var channel in category.Channels)
            {
                channelMap[channel.Channel.Id] = category.Id;

                foreach (var occupant in channel.Occupants)
                {
                    userLocation[occupant.Id] = channel.Channel.Id;
                }
            }
        }

        return (userLocation, channelMap);
    }

    public GameUser? FindUser(TownOccupants occupants, string userId)
    {
        throw new NotImplementedException();
    }

    public ChannelOccupants? FindUserChannel(TownOccupants occupants, string userId)
    {
        throw new NotImplementedException();
    }
}