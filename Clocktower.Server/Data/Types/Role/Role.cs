namespace Clocktower.Server.Data.Types.Role;

public partial record Role(string Name, string Description, RoleType Type, Edition Edition)
{
    [UsedImplicitly] public string Id => new string(Name.Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c)).ToArray()).ToLower();
    public int FirstNight { get; init; } = 0;
    public string FirstNightReminder { get; init; } = "";
    public int OtherNight { get; init; } = 0;
    public string OtherNightReminder { get; init; } = "";
    public bool Setup { get; init; } = false;
    public string SetupDescription { get; init; } = "";
    public IReadOnlyList<string> Reminders { get; init; } = [];
    public IReadOnlyList<string> RemindersGlobal { get; init; } = [];
}