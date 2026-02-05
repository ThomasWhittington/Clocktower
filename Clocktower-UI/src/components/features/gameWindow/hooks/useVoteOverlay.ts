import {useAppStore} from "@/store";
import {
    useCurrentUserIsStoryteller,
    useDiscordTown,
    useUser
} from "@/components/features/discordTownPanel/hooks";
import {useTownSquareActions} from "@/components/features/townSquare/hooks";
import {useNominationState} from "@/components/features/gameWindow/hooks";
import {
    User,
    UserType
} from "@/types";
import {
    cancelVote,
    closeNominations as callCloseNominations,
    nextNomination as callNextNomination,
    startVote,
    toggleMarkPlayer
} from "@/hooks";
import {DiscordTownUtils} from "@/utils";
import {useState} from "react";

const isPlayerEligibleToVote = (user: User | undefined) => {
    if (!user) return false;

    const isPlayer = user.userType === UserType.Player;
    const isNotLocked = !user.voteLocked;
    const canDeadVote = user.isDead ? user.hasVoteToken : true;

    return isPlayer && isNotLocked && canDeadVote;
};
export const useVoteOverlay = () => {
    const {currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const {togglePlayerVote} = useTownSquareActions();
    const {discordTown} = useDiscordTown();
    const {nominee, nominator, requiredMajority, voteUnderway, voteEnded} = useNominationState();
    const currentUserIsStoryteller = useCurrentUserIsStoryteller();
    const {gameId} = useAppStore();
    const [voteSpeed, setVoteSpeed] = useState<number>(1500);

    const player1 = DiscordTownUtils.getPlayerBySeatPosition(discordTown, nominator);
    const player2 = DiscordTownUtils.getPlayerBySeatPosition(discordTown, nominee);

    const callToggleMarkPlayer = () => {
        if (!gameId || !player2) return;
        return toggleMarkPlayer(gameId, player2.id);
    }
    const toggleVoteRunning = () => {
        if (!gameId) return;
        const action = voteUnderway ? cancelVote : startVote;
        void action(gameId, voteUnderway ? 0 : voteSpeed);
    }
    const toggleHandUp = () => {
        if (!thisUser) return;
        togglePlayerVote(thisUser);
    }

    const closeNominations = () => {
        if (!gameId) return;
        void callCloseNominations(gameId);
    }
    const nextNomination = () => {
        if (!gameId) return;
        void callNextNomination(gameId);
    }

    const canVote = isPlayerEligibleToVote(thisUser);
    const canRun = gameId != null && player1 != null && player2 != null && thisUser;

    const currentVoteCount = DiscordTownUtils.getTotalHandsUp(discordTown);
    return {
        voteUnderway,
        requiredMajority,
        canToggleVoteRunning: currentUserIsStoryteller && !voteEnded,
        canUseVoteEndedControls: currentUserIsStoryteller && voteEnded,
        canVote,
        callToggleMarkPlayer,
        toggleVoteRunning,
        toggleHandUp,
        closeNominations,
        nextNomination,
        nominatorName: player1?.name,
        nomineeName: player2?.name,
        canRun,
        currentVoteCount,
        userHandUp: thisUser?.handUp ?? false,
        userMarked: player2?.isMarked ?? false,
        setVoteSpeed,
        voteSpeed
    };
}