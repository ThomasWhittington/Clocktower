using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Data;

[UsedImplicitly, ExcludeFromCodeCoverage(Justification = "DTO")]
public record DiscordTownDto(string GameId, List<MiniCategoryDto> ChannelCategories)
{
    [UsedImplicitly] public int UserCount => TownUsers.Count();

    public IEnumerable<UserDto> TownUsers { get; } = ChannelCategories
        .SelectMany(cat => cat.Channels)
        .SelectMany(ch => ch.Occupants).ToArray();
}