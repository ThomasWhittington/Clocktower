import {useCallback, useState} from "react";

interface UseActionState {
    isLoading: boolean;
    error: string | null;
}

export function useAction() {
    const [state, setState] = useState<UseActionState>({
        isLoading: false,
        error: null,
    });

    const runAction = useCallback(async <T, >(fn: () => Promise<T>): Promise<T | undefined> => {
        setState({isLoading: true, error: null});
        try {
            const result = await fn();
            setState({isLoading: false, error: null});
            return result;
        } catch (e: unknown) {
            const message = e instanceof Error ? e.message : "Action failed";
            setState({isLoading: false, error: message});
            console.error("Action failed:", e);
            return undefined;
        }
    }, []);

    const clearError = useCallback(() => {
        setState(prev => ({...prev, error: null}));
    }, []);

    return {
        ...state,
        runAction,
        clearError,
    };
}