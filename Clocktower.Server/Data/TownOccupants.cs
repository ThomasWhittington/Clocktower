namespace Clocktower.Server.Data;

public record TownOccupants(List<MiniCategory> ChannelCategories)
{
    public int UserCount => ChannelCategories.Sum(c => c.Channels.Sum(ch => ch.Occupants.Count()));
}