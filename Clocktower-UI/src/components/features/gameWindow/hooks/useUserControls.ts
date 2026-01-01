import {useAppStore} from "@/store";
import {useCallback} from "react";
import {create} from "zustand";
import {type User, UserType} from "@/types";
import {discordService} from "@/services";

interface UserControlsStore {
    isLoading: boolean;
    error: string | null;
    result: string | null;
    runAction: (fn: () => Promise<string | undefined>) => Promise<void>;
}

//TODO implement hooks
const useUserControlsStore = create<UserControlsStore>((set) => ({
    isLoading: false,
    error: null,
    result: null,
    runAction: async (fn) => {
        set({isLoading: true, error: null, result: null});
        try {
            const res = await fn();
            set({result: res ?? null, isLoading: false});
        } catch (e: unknown) {
            const message = e instanceof Error ? e.message : "User control action failed";
            set({error: message, isLoading: false});
            throw e;
        }
    },
}));

export const useUserControls = () => {
    const {gameId} = useAppStore();
    const {isLoading, error, result, runAction} = useUserControlsStore();

    const inviteAll = useCallback(async () => {
        if (!gameId) return;
        await runAction(async () => {
            await discordService.inviteAll(gameId);
            return "All players invited";
        });
    }, [gameId, runAction]);

    const inviteUser = useCallback(async (user: User) => {
        if (!gameId) return;
        await runAction(async () => {
            await discordService.inviteUser(gameId, user.id);
            return `Invite ${user.name} clicked`;
        });
    }, [gameId, runAction]);

    const changeUserType = useCallback(async (user: User, userType: UserType) => {
        if (!gameId) return;
        await runAction(async () => {
            return `Change ${user.name} to ${UserType[userType]}`;
        });
    }, [gameId, runAction]);

    return {
        inviteAll,
        inviteUser,
        changeUserType,
        isLoading,
        error,
        result,
        canRun: Boolean(gameId) && !isLoading,
    };
}