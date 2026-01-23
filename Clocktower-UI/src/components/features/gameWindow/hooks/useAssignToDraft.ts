import {useCallback} from "react";
import {useAppStore} from "@/store";
import {useAction} from "@/hooks";
import {gamesService} from "@/services";
import {
    DiscordTown,
    Role
} from "@/types";

interface UseAssignToDraftProps {
    selectedRoles: Role[];
    discordTown: DiscordTown | undefined;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    closePanel?: () => void;
}

export const useAssignToDraft = ({
                                     selectedRoles,
                                     discordTown,
                                     setIsDraftMode,
                                     closePanel
                                 }: UseAssignToDraftProps) => {
    const {runAction} = useAction();
    const {gameId} = useAppStore();

    const assignToDraft = useCallback(async () => {
        if (!gameId) return;

        const players = discordTown?.players;
        if (!players || players.length !== selectedRoles.length) return;

        setIsDraftMode(() => true);

        const shuffledPlayers = [...players].sort(() => Math.random() - 0.5);

        const playerRoles: Record<string, string> = {};
        shuffledPlayers.forEach((player, index) => {
            playerRoles[player.id] = selectedRoles[index].id;
        });

        await runAction(async () => {
            await gamesService.setDraftRoles(gameId, playerRoles);
        });

        closePanel?.();
    }, [gameId, runAction, setIsDraftMode, closePanel, selectedRoles, discordTown?.players]);

    return {assignToDraft};
};