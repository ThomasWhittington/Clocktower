namespace Clocktower.Server.Data.Types.Role;

public static class RoleExtensions
{
    extension(Role role)
    {
        public Role FirstNight(int order, string reminder)
        {
            role.Metadata = role.Metadata with
            {
                FirstNight = order,
                FirstNightReminder = reminder
            };
            return role;
        }

        public Role OtherNight(int order, string reminder)
        {
            role.Metadata = role.Metadata with
            {
                OtherNight = order,
                OtherNightReminder = reminder
            };
            return role;
        }

        public Role EachNight(int firstOrder, int otherOther, string reminder)
        {
            role.Metadata = role.Metadata with
            {
                FirstNight = firstOrder,
                FirstNightReminder = reminder,
                OtherNight = otherOther,
                OtherNightReminder = reminder
            };
            return role;
        }

        public Role WithReminders(string[] reminders)
        {
            role.Metadata = role.Metadata with
            {
                Reminders = reminders
            };
            return role;
        }

        public Role WithReminder(string reminder)
        {
            role.Metadata = role.Metadata with
            {
                Reminders = [reminder]
            };
            return role;
        }

        public Role WithGlobalReminder(string reminder)
        {
            role.Metadata = role.Metadata with
            {
                RemindersGlobal = [reminder]
            };
            return role;
        }

        public Role AffectsSetup()
        {
            role.Metadata = role.Metadata with
            {
                Setup = true
            };
            return role;
        }
    }
}