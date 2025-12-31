import {useAppStore} from "@/store";
import {useCallback} from "react";
import {create} from "zustand";
import {type User, UserType} from "@/types";

interface UserControlsStore {
    isLoading: boolean;
    error: string | null;
    result: string | null;
    setLoading: (loading: boolean) => void;
    setResult: (result: string | null) => void;
    setError: (error: string | null) => void;
}

//TODO implement hooks
const useUserControlsStore = create<UserControlsStore>((set) => ({
    isLoading: false,
    error: null,
    result: null,
    setLoading: (isLoading) => set({isLoading}),
    setResult: (result) => set({result}),
    setError: (error) => set({error}),
}));
export const useUserControls = () => {
    const {gameId} = useAppStore();

    const {isLoading, error, result, setLoading, setResult, setError} = useUserControlsStore();

    const run = useCallback(async (fn: () => Promise<string | undefined>) => {
        setLoading(true)
        setError(null)
        setResult(null)

        try {
            const res = await fn();
            setResult(res ?? null);
        } catch (e: unknown) {
            const message = e instanceof Error ? e.message : "User control action failed";
            setError(message);
            throw e;
        } finally {
            setLoading(false);
        }
    }, []);

    const inviteAll = useCallback(async () => {
        return run(async () => {
            console.log("Invite all clicked");
            return "All players invited";
        });
    }, [gameId, run]);

    const inviteUser = useCallback(async (user: User) => {
        return run(async () => {
            console.log(`Invite ${user.name} clicked`);
            return `Invite ${user.name} clicked`;
        });
    }, [gameId, run]);

    const changeUserType = useCallback(async (user: User, userType: UserType) => {
        return run(async () => {
            console.log(`Change ${user.name} to ${UserType[userType]}`);
            return `Change ${user.name} to ${UserType[userType]}`;
        });
    }, [gameId, run]);

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