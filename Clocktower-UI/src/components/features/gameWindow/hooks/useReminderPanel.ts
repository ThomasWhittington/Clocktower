import {
    Reminder,
    User
} from "@/types";
import {
    useAction,
    useServerHub
} from "@/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {useAppStore} from "@/store";
import {useCallback} from "react";
import {gamesService} from "@/services";

export const useReminderPanel = (player: User) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const {runAction} = useAction();
    const {gameId, currentUser} = useAppStore();
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

    const setReminder = useCallback(async (targetUserId: string, reminderId: string) => {
        if (!gameId || !currentUser?.id) return;
        await runAction(async () => {
            return await gamesService.setReminder(gameId, currentUser.id, targetUserId, reminderId);
        });
    }, [gameId, runAction, currentUser?.id]);

    return {
        availableReminders: availableReminders(),
        setReminder
    };
}