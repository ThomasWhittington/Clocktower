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
import {
    discordService
} from '@/services';
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
let isInitializing = false;

const notifyTownListeners = () => {
    globalTownListeners.forEach(listener => listener({...globalTownState}));
};

const setTownState = (updates: Partial<DiscordTownState>) => {
    globalTownState = {...globalTownState, ...updates};
    notifyTownListeners();
};

const initializeDiscordTown = async (gameId: string) => {
    if (isInitializing || globalTownState.gameId === gameId) return;

    isInitializing = true;
    
    setTownState({
        isLoading: true,
        error: "",
        gameId: gameId
    });

    try {
        const data = await discordService.getDiscordTown(gameId);
        setTownState({
            discordTown: data,
            isLoading: false
        });
    } catch (err: any) {
        setTownState({
            error: err.message,
            isLoading: false
        });
    } finally {
        isInitializing = false;
    }
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

    const currentGameId = useAppStore((state) => state.gameId);
    const {discordTown: realtimeDiscordTown} = useServerHub();

    useEffect(() => {
        const listener = (newState: DiscordTownState) => setState(newState);
        listenerRef.current = listener;
        globalTownListeners.add(listener);

        if (currentGameId && currentGameId !== globalTownState.gameId) {
            initializeDiscordTown(currentGameId).then(_ => {
            });
        }

        return () => {
            if (listenerRef.current) {
                globalTownListeners.delete(listenerRef.current);
            }
        };
    }, [currentGameId]);

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
