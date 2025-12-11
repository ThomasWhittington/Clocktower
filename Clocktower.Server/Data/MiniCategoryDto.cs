namespace Clocktower.Server.Data;

[UsedImplicitly]
public record MiniCategoryDto(string Id, string Name, IEnumerable<ChannelOccupantsDto> Channels);