namespace Clocktower.Server.Data.Types.Role;

public record RoleMetadata
{
    public int FirstNight { get; init; } = 0;
    public string FirstNightReminder { get; init; } = "";
    public int OtherNight { get; init; } = 0;
    public string OtherNightReminder { get; init; } = "";
    public bool Setup { get; init; } = false;
    public IReadOnlyList<string> Reminders { get; init; } = [];
    public IReadOnlyList<string> RemindersGlobal { get; init; } = [];
}