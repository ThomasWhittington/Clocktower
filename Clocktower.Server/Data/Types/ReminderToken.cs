namespace Clocktower.Server.Data.Types;

public record ReminderToken(string RoleId, string ReminderText)
{
    [UsedImplicitly] public string Id => RoleId + "-" + ReminderText;
}