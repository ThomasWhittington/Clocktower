import type {MouseEvent} from "react";
import {
    useCallback,
    useMemo,
    useState
} from "react";
import type {User} from "@/types";
import {gamesService} from "@/services";
import {useAppStore} from "@/store";
import {
    makeNomination,
    startVote as startVoting,
    toggleVote,
    useAction,
} from "@/hooks";

export function useTownSquareActions() {
    const [activeMenuPlayerId, setActiveMenuPlayerId] = useState<string | null>(null);
    const [swappingPlayer, setSwappingPlayer] = useState<User | null>(null);
    const [nominatingPlayer, setNominatingPlayer] = useState<User | null>(null);
    const {runAction} = useAction();
    const {gameId, currentUser} = useAppStore();

    const closeMenu = useCallback(() => {
        setActiveMenuPlayerId(null);
    }, []);

    const toggleMenu = useCallback((playerId: string, e?: MouseEvent) => {
        e?.stopPropagation();
        setActiveMenuPlayerId((prev) => (prev === playerId ? null : playerId));
    }, []);

    const initiateSwap = useCallback((player: User) => {
        setNominatingPlayer(null);
        setSwappingPlayer(player);
        setActiveMenuPlayerId(null);
    }, []);

    const confirmSwap = useCallback(async (target: User) => {
        if (swappingPlayer && gameId) {
            const result = await runAction(async () => {
                return await gamesService.swapSeatingPositions(gameId, swappingPlayer.id, target.id);
            });
            if (result) {
                setSwappingPlayer(null);
            }
        } else {
            setSwappingPlayer(null);
        }
    }, [swappingPlayer, gameId, runAction]);

    const cancelSwap = useCallback(() => {
        setSwappingPlayer(null);
    }, []);


    const initiateNomination = useCallback((player: User) => {
        setSwappingPlayer(null);
        setNominatingPlayer(player);
        setActiveMenuPlayerId(null);
    }, []);

    const confirmNomination = useCallback(async (target: User) => {
        if (nominatingPlayer && gameId) {
            const result = await runAction(async () => {
                return await makeNomination(gameId, nominatingPlayer.id, target.id);
            });
            if (result) {
                setNominatingPlayer(null);
            }
        } else {
            setNominatingPlayer(null);
        }
    }, [nominatingPlayer, gameId, runAction]);

    const cancelNomination = useCallback(() => {
        setNominatingPlayer(null);
    }, []);

    const playerNominatesPlayer = useCallback(async (target: User) => {
        if (gameId && currentUser) {
            setSwappingPlayer(null);
            await runAction(async () => {
                return await makeNomination(gameId, currentUser.id, target.id);
            });
        }
    }, [gameId, currentUser, runAction]);

    const startVote = useCallback(() => {
        if (gameId) {
            void startVoting(gameId, 1000);
        }
    }, [gameId]);


    const togglePlayerVote = useCallback((player: User) => {
        if (gameId) {
            void toggleVote(gameId, player.id);
        }
    }, [gameId]);

    return useMemo(() => ({
        activeMenuPlayerId,
        swappingPlayer,
        toggleMenu,
        closeMenu,
        initiateSwap,
        confirmSwap,
        cancelSwap,
        startVote,
        nominatingPlayer,
        initiateNomination,
        confirmNomination,
        cancelNomination,
        playerNominatesPlayer,
        togglePlayerVote
    }), [
        activeMenuPlayerId,
        swappingPlayer,
        toggleMenu,
        closeMenu,
        initiateSwap,
        confirmSwap,
        cancelSwap,
        startVote,
        nominatingPlayer,
        initiateNomination,
        confirmNomination,
        cancelNomination,
        playerNominatesPlayer,
        togglePlayerVote
    ]);
}