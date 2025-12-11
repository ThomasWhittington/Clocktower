using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Data;

[UsedImplicitly, ExcludeFromCodeCoverage(Justification = "DTO")]
public record ChannelOccupantsDto(MiniChannel Channel, IEnumerable<UserDto> Occupants);