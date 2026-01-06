import {useState, useCallback} from "react";
import type {User} from "@/types";
import type {MouseEvent} from "react";
import {useUserControls} from "@/components/features/gameWindow/hooks";
import {gamesService} from "@/services";
import {useAppStore} from "@/store";

export function useTownSquareActions() {
    const [activeMenuPlayerId, setActiveMenuPlayerId] = useState<string | null>(null);
    const [swappingPlayer, setSwappingPlayer] = useState<User | null>(null);
    const {runAction} = useUserControls();
    const {gameId} = useAppStore();

    const closeMenu = useCallback(() => {
        setActiveMenuPlayerId(null);
    }, []);

    const toggleMenu = useCallback((playerId: string, e?: MouseEvent) => {
        e?.stopPropagation();
        setActiveMenuPlayerId((prev) => (prev === playerId ? null : playerId));
    }, []);

    const initiateSwap = useCallback((player: User) => {
        setSwappingPlayer(player);
        setActiveMenuPlayerId(null);
    }, []);

    const confirmSwap = useCallback(async (target: User) => {
        if (swappingPlayer && gameId) {
            await runAction(async () => {
                return await gamesService.swapSeatingPositions(gameId, swappingPlayer.id, target.id);
            });
        }
        setSwappingPlayer(null);
    }, [swappingPlayer, gameId, runAction]);

    const cancelSwap = useCallback(() => {
        setSwappingPlayer(null);
    }, []);

    return {
        activeMenuPlayerId,
        swappingPlayer,
        toggleMenu,
        closeMenu,
        initiateSwap,
        confirmSwap,
        cancelSwap,
    };
}