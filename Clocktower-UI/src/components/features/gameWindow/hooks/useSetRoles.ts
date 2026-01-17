import {useCallback} from "react";
import {Role} from "@/types";
import {useAction} from "@/hooks";
import {useAppStore} from "@/store";
import {gamesService} from "@/services";

export function useSetRoles(currentUserId: string, targetUserId: string) {
    const {runAction} = useAction();
    const {gameId} = useAppStore();

    const setRole = useCallback(async (role: Role | undefined, isDraftMode: boolean) => {
        if (!gameId) return;
        await runAction(async () => {
            if (isDraftMode) {
                return await gamesService.setDraftRole(gameId, currentUserId, targetUserId, role?.id);
            } else {
                return await gamesService.setRole(gameId, currentUserId, targetUserId, role?.id);
            }
        });
    }, [gameId, runAction, currentUserId, targetUserId]);

    return {
        setRole
    };
}