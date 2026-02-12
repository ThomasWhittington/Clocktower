import {
    Reminder,
    User
} from "@/types";
import {useServerHub} from "@/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

export const useReminderPanel = (player: User) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const availableReminders = (): Reminder[] => {
        if (!script) return [];
        const reminders: Reminder[] = [];
        const globalReminderRoles = script?.roles.filter(o => o.remindersGlobal.length > 0);
        for (const role of globalReminderRoles) {
            role.remindersGlobal.forEach((reminder: string) => {
                reminders.push({id: role.id + "-" + reminder, roleId: role.id, reminderText: reminder});
            });
        }

        discordTown?.players?.forEach(player => {
            player.role?.reminders.forEach(reminder => {
                reminders.push({id: player.role!.id + "-" + reminder, roleId: player.role!.id, reminderText: reminder});
            });
        });
        return reminders.filter(reminder =>
            !player.reminderTokens.some(token => token.id === reminder.id)
        );
    };

    return {
        availableReminders: availableReminders()
    };
}