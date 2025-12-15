import {
    useCallback,
    useState
} from "react";
import {
    useAppStore
} from "@/store";
import {
    adminService
} from "@/services";

type TimerActionsState = {
    isLoading: boolean;
    error: string | null;
    result: string | null;
};

export const useTimerActions = () => {
    const {gameId} = useAppStore();

    const [state, setState] = useState<TimerActionsState>({
        isLoading: false,
        error: null,
        result: null
    });

    const run = useCallback(async (fn: () => Promise<string | undefined>) => {
        setState({
            isLoading: true,
            error: null,
            result: null
        });

        try {
            const result = await fn();
            setState((s) => ({
                ...s,
                result: result ?? null
            }));
        } catch (e: unknown) {
            const message = e instanceof Error ? e.message : "Timer action failed";
            setState({
                isLoading: false,
                error: message,
                result: null
            });
            throw e;
        } finally {
            setState((s) => ({
                ...s,
                isLoading: false
            }));
        }
    }, []);

    const startOrEditTimer = useCallback(async (durationSeconds: number, label?: string) => {
        if (!gameId) return;

        await run(async () => {
            await adminService.startOrEditTimer(gameId, durationSeconds, label);
            const mins = Math.floor(durationSeconds / 60);
            const secs = durationSeconds % 60;
            return `Timer set to ${mins}:${String(secs).padStart(2, "0")}`;
        });
    }, [gameId, run]);

    const cancelTimer = useCallback(async () => {
        if (!gameId) return;

        await run(async () => {
            await adminService.cancelTimer(gameId);
            return "Timer cancelled";
        });
    }, [gameId, run]);

    return {
        startOrEditTimer,
        cancelTimer,
        isLoading: state.isLoading,
        result: state.result,
        error: state.error,
        canRun: Boolean(gameId) && !state.isLoading,
    };
};