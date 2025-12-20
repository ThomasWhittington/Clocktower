using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Data;

[UsedImplicitly, ExcludeFromCodeCoverage(Justification = "DTO")]
public record GamePerspectiveDto(string Id, string GuildId, GameUserDto CreatedBy, DateTime CreatedDate) : IIdentifiable
{
    public string Id { get; } = Id;
    public string GuildId { get; } = GuildId;
    public GameUserDto CreatedBy { get; } = CreatedBy;
    public DateTime CreatedDate { get; } = CreatedDate;
    public IReadOnlyList<GameUserDto> Users { get; init; } = [];
    public GameTime GameTime { get; init; }
}