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
import {
    ValidationUtils
} from '@/utils';
import type {
    DiscordTown
} from '@/types';

type DiscordTownState = {
    discordTown?: DiscordTown;
    isLoading: boolean;
    error: string;
    guildId?: string;
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

const initializeDiscordTown = async (guildId: string) => {
    if (isInitializing || globalTownState.guildId === guildId) return;

    isInitializing = true;

    if (!ValidationUtils.isValidDiscordId(guildId)) {
        console.error('guildId was not valid');
        setTownState({
            error: 'Invalid guild ID',
            isLoading: false
        });
        isInitializing = false;
        return;
    }

    setTownState({
        isLoading: true,
        error: "",
        guildId
    });

    try {
        const data = await discordService.getDiscordTown(guildId);
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
        guildId: undefined,
        discordTown: undefined
    };
    notifyTownListeners();
};

export const useDiscordTown = () => {
    const [state, setState] = useState<DiscordTownState>(globalTownState);
    const listenerRef = useRef<(state: DiscordTownState) => void>(null);

    const guildId = useAppStore((state) => state.guildId);
    const {discordTown: realtimeDiscordTown} = useServerHub();

    useEffect(() => {
        const listener = (newState: DiscordTownState) => setState(newState);
        listenerRef.current = listener;
        globalTownListeners.add(listener);

        if (guildId && guildId !== globalTownState.guildId) {
            initializeDiscordTown(guildId).then(_ => {
            });
        }

        return () => {
            if (listenerRef.current) {
                globalTownListeners.delete(listenerRef.current);
            }
        };
    }, [guildId]);

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
