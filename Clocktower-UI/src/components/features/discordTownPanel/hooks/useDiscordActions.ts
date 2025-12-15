import {
    useCallback,
    useState
} from "react";
import {
    useAppStore
} from "@/store";
import {
    discordService
} from "@/services";

type DiscordActionsState = {
    isLoading: boolean;
    error: string | null;
    result: string | null;
};

export const useDiscordActions = () => {
    const {gameId} = useAppStore();

    const [state, setState] = useState<DiscordActionsState>({
        isLoading: false,
        error: null,
        result: null
    });

    const run = useCallback(
        async (fn: () => Promise<string | undefined>) => {
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
                const message = e instanceof Error ? e.message : "Discord action failed";
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
        },
        []
    );

    const sendToCottages = useCallback(async () => {
        if (!gameId) return;

        await run(async () => {
            return await discordService.sendToCottages(gameId);
        });
    }, [gameId, run]);

    const sendToTownSquare = useCallback(async () => {
        if (!gameId) return;

        await run(async () => {
            return await discordService.sendToTownSquare(gameId);
        });
    }, [gameId, run]);

    return {
        sendToCottages,
        sendToTownSquare,
        isLoading: state.isLoading,
        result: state.result,
        error: state.error,
        canRun: !!gameId && !state.isLoading,
    };
};