namespace Clocktower.Server.Data;

public record DiscordTown(List<MiniCategory> ChannelCategories)
{
    public int UserCount => TownUsers.Count();

    public IEnumerable<TownUser> TownUsers => ChannelCategories
        .SelectMany(cat => cat.Channels)
        .SelectMany(ch => ch.Occupants);
}