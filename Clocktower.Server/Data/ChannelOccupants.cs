namespace Clocktower.Server.Data;

[UsedImplicitly]
public record ChannelOccupants(MiniChannel Channel, IEnumerable<TownUser> Occupants);