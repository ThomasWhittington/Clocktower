import {useCallback} from "react";
import {Role, type User} from "@/types";
import {useAction} from "@/hooks";
import {useAppStore} from "@/store";
import {gamesService} from "@/services";

export function useSetRoles(player: User) {
    const {runAction} = useAction();
    const {gameId} = useAppStore();

    const setRole = useCallback(async (role: Role | undefined, isDraftMode: boolean) => {
        if (!gameId) return;
        await runAction(async () => {
            if (isDraftMode) {
                return await gamesService.setDraftRole(gameId, player.id, role?.id);
            } else {
                return await gamesService.setRole(gameId, player.id, role?.id);
            }
        });
    }, [player.role, gameId, runAction]);

    return {
        setRole
    };
}