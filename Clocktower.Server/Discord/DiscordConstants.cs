using System.Diagnostics.CodeAnalysis;

namespace Clocktower.Server.Discord;

public interface IDiscordConstantsService
{
    string TownSquareName { get; }
    string ConsultationName { get; }
    string DayCategoryName { get; }
    string NightCategoryName { get; }
    string CottageName { get; }
    string StoryTellerRoleName { get; }
    string PlayerRoleName { get; }
    string SpectatorRoleName { get; }
    int CottageCount { get; }
    string[] DayRoomNames { get; }
    string[] GetNightRoomNames();
}

[ExcludeFromCodeCoverage(Justification = "Mockable constants class")]
public class DiscordConstantsService : IDiscordConstantsService
{
    public string TownSquareName => "⛲ Town Square";
    public string ConsultationName => "📖 Storyteller's Consultation";
    public string DayCategoryName => "🌞 Day BOTC";
    public string NightCategoryName => "🌙 Night BOTC ✨";
    public string CottageName => "🛌 Cottage";
    public string StoryTellerRoleName => "StoryTeller";
    public string PlayerRoleName => "Player";
    public string SpectatorRoleName => "Spectator";
    public int CottageCount => 15;

    public string[] GetNightRoomNames() => Enumerable.Range(0, CottageCount).Select(_ => CottageName).ToArray();

    public string[] DayRoomNames =>
    [
        TownSquareName,
        "🍻 Inn",
        "🏫 School",
        "⛪ Church",
        "🔱 Devil's Lair",
        "🌳 Forbidden Forest",
        "🏰 Lost Castle",
        "🗡 Village Smithy",
        "🕍 Sacred Temple",
        "💀 Haunted Cemetery",
        ConsultationName
    ];
}