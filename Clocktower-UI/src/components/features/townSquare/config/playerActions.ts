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
    currentUserId?: string;
}

export interface PlayerActionContext {
    initiateSwap: (player: User) => void;
    toggleMarkPlayer: (player: User) => void;
    initiateNomination: (player: User) => void;
    requestToTalk: (player: User) => Promise<void>;
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
            id: "toggle-mark",
            label: "Toggle Mark",
            icon: "💀️",
            isVisible: (_, state) => state.currentUserIsStoryTeller,
            execute: (player, {toggleMarkPlayer}) => {
                toggleMarkPlayer(player);
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
        id: "request-to-talk",
        label: "Request to Talk",
        icon: "💬",
        isVisible: (user, state) => state.currentUserId !== undefined && state.currentUserId !== user.id,
        execute: (player, {requestToTalk}) => {
            void requestToTalk(player);
        },
        }
    ]
;