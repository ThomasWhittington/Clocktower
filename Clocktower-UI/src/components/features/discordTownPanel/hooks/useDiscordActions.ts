import {useCallback, useState} from "react";
import {useAppStore} from "@/store";
import {discordService} from "@/services";
import {useTimeOfDay} from "@/components/features/discordTownPanel/hooks/useTimeOfDay.ts";
import {GameTime} from "@/types";

type DiscordActionsState = {
    isLoading: boolean;
    error: string | null;
    result: string | null;
};

export const useDiscordActions = () => {
    const {gameId} = useAppStore();
    const setTime = useTimeOfDay();

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
            await setTime(GameTime.Night);
            return await discordService.sendToCottages(gameId);
        });
    }, [gameId, run]);

    const sendToTownSquare = useCallback(async () => {
        if (!gameId) return;

        await run(async () => {
            await setTime(GameTime.Day);
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