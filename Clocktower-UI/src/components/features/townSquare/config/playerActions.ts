import type {User} from "@/types";
import {UserUtils} from "@/utils";

export interface PlayerAction {
    id: string;
    label: string;
    icon: string;
    isVisible: (player: User, currentUser: User | undefined) => boolean;
    execute: (player: User, context: PlayerActionContext) => void | Promise<void>;
}

export interface PlayerActionContext {
    gameId: string;
    currentUser: User | undefined;
    initiateSwap: (player: User) => void;
}

export const playerActions: PlayerAction[] = [
    {
        id: "swap-seats",
        label: "Swap Seats",
        icon: "🔄",
        isVisible: (player, currentUser) => UserUtils.isStoryTeller(currentUser),
        execute: (player, {initiateSwap}) => {
            initiateSwap(player);
        },
    }
];