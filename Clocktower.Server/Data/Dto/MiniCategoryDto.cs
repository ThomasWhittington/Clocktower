namespace Clocktower.Server.Data.Dto;

[UsedImplicitly]
public record MiniCategoryDto(string Id, string Name, IEnumerable<ChannelOccupantsDto> Channels);