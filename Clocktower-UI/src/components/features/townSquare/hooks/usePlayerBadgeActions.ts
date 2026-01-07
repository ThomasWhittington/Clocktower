import type {MouseEvent} from "react";
import {useCallback} from "react";
import type {User} from "@/types";
import {useAction} from "@/hooks";
import {useAppStore} from "@/store";
import {gamesService} from "@/services";

export function usePlayerBadgeActions(player: User) {
    const {runAction} = useAction();
    const {gameId} = useAppStore();

    const handleShroudClick = useCallback(async (e: MouseEvent) => {
        e.stopPropagation();
        if (!gameId) return;

        await runAction(async () => {
            const result = await gamesService.setPlayerIsDead(gameId, player.id, !player.isDead);
            console.log(result);
            return result;
        });
    }, [player.isDead, player.id, gameId, runAction]);

    const handleVoteTokenClick = useCallback(async (e: MouseEvent) => {
        e.stopPropagation();
        if (!gameId) return;

        await runAction(async () => {
            const result = await gamesService.setPlayerHasVoteToken(gameId, player.id, !player.hasVoteToken);
            console.log(result);
            return result;
        });
    }, [player.hasVoteToken, player.isDead, player.id, gameId, runAction]);

    return {
        handleShroudClick,
        handleVoteTokenClick,
    };
}