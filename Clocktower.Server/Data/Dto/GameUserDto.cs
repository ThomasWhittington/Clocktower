using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Data.Dto;

[UsedImplicitly, ExcludeFromCodeCoverage(Justification = "DTO")]
public record GameUserDto(string Id) : IIdentifiable;