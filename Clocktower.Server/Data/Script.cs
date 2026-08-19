namespace Clocktower.Server.Data;

public record Script(string Name, string Author, IReadOnlyList<Role> Roles)
{
    public IReadOnlyList<PermanentReminder> PermanentReminders { get; init; } =
    [
        new("good", "Good"),
        new("evil", "Evil")
    ];
}

public record PermanentReminder(string RoleId, string ReminderText);