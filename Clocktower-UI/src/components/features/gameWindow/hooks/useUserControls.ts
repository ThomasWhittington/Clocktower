import {useAppStore} from "@/store";
import {useCallback} from "react";
import {create} from "zustand";
import {type User, UserType} from "@/types";
import {discordService, gamesService} from "@/services";

interface UserControlsStore {
    isLoading: boolean;
    error: string | null;
    result: string | null;
    availableUsers: User[];
    setAvailableUsers: (users: User[]) => void;
    runAction: <T>(fn: () => Promise<T>) => Promise<T | undefined>;
}

const useUserControlsStore = create<UserControlsStore>((set) => ({
    isLoading: false,
    error: null,
    result: null,
    availableUsers: [],
    setAvailableUsers: (availableUsers) => set({availableUsers}),
    runAction: async (fn) => {
        set({isLoading: true, error: null, result: null});
        try {
            const res = await fn();
            const statusMessage = typeof res === "string" ? res : null;
            set({result: statusMessage ?? null, isLoading: false});
            return res;
        } catch (e: unknown) {
            const message = e instanceof Error ? e.message : "User control action failed";
            set({error: message, isLoading: false});
            throw e;
        }
    },
}));

export const useUserControls = () => {
    const {gameId} = useAppStore();
    const {isLoading, error, result, availableUsers, setAvailableUsers, runAction} = useUserControlsStore();

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
            return `Invite ${user.name}`;
        });
    }, [gameId, runAction]);
    const addUserToGame = useCallback(async (user: User) => {
        if (!gameId) return;
        await runAction(async () => {
            await gamesService.addUserToGame(gameId, user.id);
            return `Added ${user.name}`;
        });
    }, [gameId, runAction]);
    const changeUserType = useCallback(async (user: User, userType: UserType) => {
        if (!gameId) return;
        await runAction(async () => {
            return `Change ${user.name} to ${UserType[userType]}`;
        });
    }, [gameId, runAction]);

    const getAvailableGameUsers = useCallback(async () => {
        if (!gameId) return;
        const users = await runAction(async () => {
            return await gamesService.getAvailableGameUsers(gameId);
        });
        if (Array.isArray(users)) {
            setAvailableUsers(users);
        }
    }, [gameId, runAction, setAvailableUsers]);

    return {
        inviteAll,
        inviteUser,
        changeUserType,
        getAvailableGameUsers,
        addUserToGame,
        availableUsers,
        isLoading,
        error,
        result,
        canRun: Boolean(gameId) && !isLoading,
    };
}