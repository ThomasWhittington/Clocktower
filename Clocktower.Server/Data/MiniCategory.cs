namespace Clocktower.Server.Data;

[UsedImplicitly]
public record MiniCategory(string Id, string Name, IEnumerable<ChannelOccupants> Channels);