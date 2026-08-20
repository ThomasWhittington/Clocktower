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

export const useReminders = (player?: User) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const {runAction} = useAction();
    const {gameId, currentUser} = useAppStore();
    const availableReminders = (): Reminder[] => {
        if (!script) return [];
        const reminders: Reminder[] = [];
        const seenIds = new Set<string>();

        script.permanentReminders.forEach(({roleId, reminderText}) => {
            const id = roleId + "-" + reminderText;
            if (!seenIds.has(id)) {
                seenIds.add(id);
                reminders.push({id, roleId, reminderText});
            }
        });

        const globalReminderRoles = script?.roles.filter(o => o.remindersGlobal.length > 0);
        for (const role of globalReminderRoles) {
            role.remindersGlobal.forEach((reminder: string) => {
                const id = role.id + "-" + reminder;
                if (!seenIds.has(id)) {
                    seenIds.add(id);
                    reminders.push({id, roleId: role.id, reminderText: reminder});
                }
            });
        }

        discordTown?.players?.forEach(townPlayer => {
            townPlayer.role?.reminders.forEach(reminder => {
                const id = townPlayer.role!.id + "-" + reminder;
                if (!seenIds.has(id)) {
                    seenIds.add(id);
                    reminders.push({id, roleId: townPlayer.role!.id, reminderText: reminder});
                }
            });
        });

        return reminders.filter(reminder =>
            !player?.reminderTokens.some(token => token.id === reminder.id)
        );
    };
    const setReminder = useCallback(async (targetUserId: string, reminderId: string) => {
        if (!gameId || !currentUser?.id) return;
        await runAction(async () => {
            return await gamesService.setReminder(gameId, currentUser.id, targetUserId, reminderId);
        });
    }, [gameId, runAction, currentUser?.id]);
    const removeReminder = useCallback(async (targetUserId: string, reminderId: string) => {
        if (!gameId || !currentUser?.id) return;
        await runAction(async () => {
            return await gamesService.removeReminder(gameId, currentUser.id, targetUserId, reminderId);
        });
    }, [gameId, runAction, currentUser?.id]);
    const addCustomReminder = useCallback(async (targetUserId: string, reminderText: string) => {
        if (!gameId || !currentUser?.id) return;
        await runAction(async () => {
            return await gamesService.setCustomReminder(gameId, currentUser.id, targetUserId, reminderText);
        });
    }, [gameId, runAction, currentUser?.id]);

    return {
        availableReminders: availableReminders(),
        setReminder,
        removeReminder,
        addCustomReminder
    };
}