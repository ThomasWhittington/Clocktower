namespace Clocktower.Server.Data.Types.Role;

public static class RoleExtensions
{
    extension(Role role)
    {
        public Role FirstNight(int order, string reminder)
        {
            return role with
            {
                FirstNight = order,
                FirstNightReminder = reminder
            };
        }

        public Role OtherNight(int order, string reminder)
        {
            return role with
            {
                OtherNight = order,
                OtherNightReminder = reminder
            };
        }

        public Role EachNight(int firstOrder, int otherOther, string reminder)
        {
            return role with
            {
                FirstNight = firstOrder,
                FirstNightReminder = reminder,
                OtherNight = otherOther,
                OtherNightReminder = reminder
            };
        }

        public Role WithReminders(params string[] reminders)
        {
            return role with
            {
                Reminders = reminders
            };
        }

        public Role WithGlobalReminders(params string[] reminders)
        {
            return role with
            {
                RemindersGlobal = reminders
            };
        }

        public Role AffectsSetup()
        {
            return role with
            {
                Setup = true
            };
        }
    }
}