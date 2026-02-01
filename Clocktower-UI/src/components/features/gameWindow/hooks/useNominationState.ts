import {
    closeNominations,
    openNominations,
    useServerHub
} from "@/hooks";
import {useAppStore} from "@/store";

export const useNominationState = () => {
    const {nominationSession} = useServerHub();
    const {gameId} = useAppStore();

    const nominationsEnabled = nominationSession !== undefined;
    const isActiveNomination = nominationSession?.isActiveNomination ?? false;
    const voteUnderway = nominationSession?.voteUnderway ?? false;
    const toggleNominations = () => {
        if (!gameId) return;
        if (nominationsEnabled) {
            void closeNominations(gameId);
        } else {
            void openNominations(gameId);
        }
    }
    return {
        nominationsEnabled,
        isActiveNomination,
        voteUnderway,
        toggleNominations
    };
};