import {
    useEffect,
    useRef,
    useState
} from 'react';
import {
    useAppStore
} from '@/store';
import {
    useDiscordHub
} from './useDiscordHub';
import {
    discordService
} from '@/services';
import {
    ValidationUtils
} from '@/utils';
import type {
    TownOccupants
} from '@/types';

type TownOccupancyState = {
    townOccupancy?: TownOccupants;
    isLoading: boolean;
    error: string;
    guildId?: string;
};

let globalTownState: TownOccupancyState = {
    isLoading: false,
    error: ""
};
const globalTownListeners = new Set<(state: TownOccupancyState) => void>();
let isInitializing = false;

const notifyTownListeners = () => {
    globalTownListeners.forEach(listener => listener({...globalTownState}));
};

const setTownState = (updates: Partial<TownOccupancyState>) => {
    globalTownState = {...globalTownState, ...updates};
    notifyTownListeners();
};

const initializeTownOccupancy = async (guildId: string) => {
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
        const data = await discordService.getTownOccupancy(guildId);
        setTownState({
            townOccupancy: data,
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

const resetTownOccupancyState = () => {
    globalTownState = {
        isLoading: false,
        error: "",
        guildId: undefined,
        townOccupancy: undefined
    };
    notifyTownListeners();
};

export const useTownOccupancy = () => {
    const [state, setState] = useState<TownOccupancyState>(globalTownState);
    const listenerRef = useRef<(state: TownOccupancyState) => void>(null);

    const guildId = useAppStore((state) => state.guildId);
    const {townOccupancy: realtimeTownOccupancy} = useDiscordHub();

    useEffect(() => {
        const listener = (newState: TownOccupancyState) => setState(newState);
        listenerRef.current = listener;
        globalTownListeners.add(listener);

        if (guildId && guildId !== globalTownState.guildId) {
            initializeTownOccupancy(guildId).then(_ => {
            });
        }

        return () => {
            if (listenerRef.current) {
                globalTownListeners.delete(listenerRef.current);
            }
        };
    }, [guildId]);

    useEffect(() => {
        if (realtimeTownOccupancy) {
            setTownState({townOccupancy: realtimeTownOccupancy});
        }
    }, [realtimeTownOccupancy]);

    return {
        townOccupancy: state.townOccupancy,
        isLoading: state.isLoading,
        error: state.error
    };
};

export const resetTownOccupancy = resetTownOccupancyState;
