import type {User} from "@/types";

export interface PlayerAction {
    id: string;
    label: string;
    icon: string;
    isVisible: (player: User, state: PlayerActionState) => boolean;
    execute: (player: User, context: PlayerActionContext) => void | Promise<void>;
}

export interface PlayerActionState {
    nominationsEnabled: boolean;
    currentUserIsStoryTeller: boolean;
}

export interface PlayerActionContext {
    initiateSwap: (player: User) => void;
    startVote: (player: User) => void;
    initiateNomination: (player: User) => void;
    playerNominatesPlayer: (target: User) => Promise<void>;
}

export const playerActions: PlayerAction[] = [
        {
            id: "swap-seats",
            label: "Swap Seats",
            icon: "🔄",
            isVisible: (_, state) => state.currentUserIsStoryTeller,
            execute: (player, {initiateSwap}) => {
                initiateSwap(player);
            },
        },
        {
            id: "start-vote",
            label: "Start Vote",
            icon: "🗳️",
            isVisible: (_, state) => state.currentUserIsStoryTeller && state.nominationsEnabled,
            execute: (player, {startVote}) => {
                startVote(player);
            },
        },
        {
            id: "storyteller-nomination",
            label: "Nomination",
            icon: "🗳️",
            isVisible: (_, state) => state.currentUserIsStoryTeller && state.nominationsEnabled,
            execute: (player, {initiateNomination}) => {
                initiateNomination(player);
            },
        },
        {
            id: "player-nomination",
            label: "Nominate",
            icon: "🗳️",
            isVisible: (player, state) => {
                return !state.currentUserIsStoryTeller && state.nominationsEnabled && !player.isDead;
            },
            execute: (player, {playerNominatesPlayer}) => {
                void playerNominatesPlayer(player);
            },
        }
    ]
;