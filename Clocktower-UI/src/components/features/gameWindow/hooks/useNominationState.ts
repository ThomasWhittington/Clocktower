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
    const voteEnded = nominationSession?.voteEnded ?? false;
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
        voteEnded,
        toggleNominations,
        nominee: nominationSession?.nominee,
        nominator: nominationSession?.nominator,
        requiredMajority: nominationSession?.requiredMajority ?? 0
    };
};