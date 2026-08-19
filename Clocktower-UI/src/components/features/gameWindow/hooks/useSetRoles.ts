import {useCallback} from "react";
import {Role} from "@/types";
import {useAction} from "@/hooks";
import {useAppStore} from "@/store";
import {gamesService} from "@/services";

export function useSetRoles(currentUserId: string, isStoryTeller: boolean, isDraftMode: boolean) {
    const {runAction} = useAction();
    const {gameId} = useAppStore();

    const setRole = useCallback(async (role: Role | undefined, targetUserId: string) => {
        if (!gameId) return;
        await runAction(async () => {
            if (isDraftMode && !isStoryTeller) {
                throw new Error("Only storytellers can set draft roles");
            }
            if (isDraftMode) {
                return await gamesService.setDraftRole(gameId, targetUserId, role?.id);
            } else if (isStoryTeller) {
                return await gamesService.setRole(gameId, targetUserId, role?.id);
            } else {
                return await gamesService.setPerspectiveRole(gameId, currentUserId, targetUserId, role?.id);
            }
        });
    }, [gameId, runAction, currentUserId, isDraftMode, isStoryTeller]);

    const commitDraftRoles = useCallback(async () => {
        if (!gameId) return;
        await runAction(async () => {
            return await gamesService.commitDraft(gameId);
        });
    }, [gameId, runAction]);

    return {
        setRole,
        commitDraftRoles
    };
}