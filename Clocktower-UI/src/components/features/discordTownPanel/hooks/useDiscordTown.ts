import {
    useEffect,
    useRef,
    useState
} from 'react';
import {
    useAppStore
} from '@/store';
import {
    useServerHub
} from "@/hooks";
import type {
    DiscordTown
} from '@/types';

type DiscordTownState = {
    discordTown?: DiscordTown;
    isLoading: boolean;
    error: string;
    gameId?: string;
};

let globalTownState: DiscordTownState = {
    isLoading: false,
    error: ""
};
const globalTownListeners = new Set<(state: DiscordTownState) => void>();

const notifyTownListeners = () => {
    globalTownListeners.forEach(listener => listener({...globalTownState}));
};

const setTownState = (updates: Partial<DiscordTownState>) => {
    globalTownState = {...globalTownState, ...updates};
    notifyTownListeners();
};

const resetDiscordTownState = () => {
    globalTownState = {
        isLoading: false,
        error: "",
        gameId: undefined,
        discordTown: undefined
    };
    notifyTownListeners();
};

export const useDiscordTown = () => {
    const [state, setState] = useState<DiscordTownState>(globalTownState);
    const listenerRef = useRef<(state: DiscordTownState) => void>(null);

    const {gameId} = useAppStore();
    const {discordTown: realtimeDiscordTown} = useServerHub();

    useEffect(() => {
        const listener = (newState: DiscordTownState) => setState(newState);
        listenerRef.current = listener;
        globalTownListeners.add(listener);
        return () => {
            if (listenerRef.current) {
                globalTownListeners.delete(listenerRef.current);
            }
        };
    }, [gameId]);

    useEffect(() => {
        if (realtimeDiscordTown) {
            setTownState({discordTown: realtimeDiscordTown});
        }
    }, [realtimeDiscordTown]);

    return {
        discordTown: state.discordTown,
        isLoading: state.isLoading,
        error: state.error
    };
};

export const resetDiscordTown = resetDiscordTownState;
