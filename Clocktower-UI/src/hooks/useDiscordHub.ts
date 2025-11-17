import {
    useEffect,
    useRef,
    useState
} from 'react';
import * as signalR
    from '@microsoft/signalr';
import {
    HubConnectionState
} from '@microsoft/signalr';
import type {
    TownOccupants
} from '@/types';
import {
    useAppStore
} from "@/store";

type UserVoiceStates = Record<string, boolean>;

export enum GameTime {
    Day = 0,
    Evening = 1,
    Night = 2,
}

type DiscordHubState = {
    townOccupancy?: TownOccupants;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime
};

let globalConnection: signalR.HubConnection | null = null;
let globalState: DiscordHubState = {
    userVoiceStates: {},
    connectionState: signalR.HubConnectionState.Disconnected,
    gameTime: GameTime.Night
};
const globalListeners = new Set<(state: DiscordHubState) => void>();

const notifyListeners = () => {
    globalListeners.forEach(listener => listener({...globalState}));
};

const setState = (updates: Partial<DiscordHubState>) => {
    globalState = {...globalState, ...updates};
    notifyListeners();
};

const resetDiscordHubState = () => {
    globalState = {
        userVoiceStates: {},
        connectionState: HubConnectionState.Disconnected,
        gameTime: GameTime.Night
    };
    notifyListeners();
};

const createConnection = async (jwt?: string) => {
    if (globalConnection) return;

    globalConnection = new signalR.HubConnectionBuilder()
        .withUrl(import.meta.env.VITE_CLOCKTOWER_SERVER_URI + '/discordHub', {
            accessTokenFactory: () => jwt ?? ''
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    globalConnection.on('TownOccupancyUpdated', (occupants: TownOccupants) => {
        setState({townOccupancy: occupants});
    });

    globalConnection.on('TownTimeChanged', (gameTime: number) => {
        setState({gameTime: gameTime as GameTime});
    });

    globalConnection.on('PingUser', (message: string) => {
        console.log(`ping from server: ${message}`);
    });

    globalConnection.on('UserVoiceStateChanged', (userId: string, isInVoice: boolean) => {
        setState({
            userVoiceStates: {
                ...globalState.userVoiceStates,
                [userId]: isInVoice
            }
        });
    });

    globalConnection.onclose(() => setState({connectionState: signalR.HubConnectionState.Disconnected}));
    globalConnection.onreconnecting(() => setState({connectionState: signalR.HubConnectionState.Reconnecting}));
    globalConnection.onreconnected(() => setState({connectionState: signalR.HubConnectionState.Connected}));

    try {
        await globalConnection.start();
        setState({connectionState: signalR.HubConnectionState.Connected});
    } catch (error) {
        console.error('SignalR connection failed:', error);
        setState({connectionState: signalR.HubConnectionState.Disconnected});
    }
};

export const useDiscordHub = () => {
    const [state, setState] = useState<DiscordHubState>(globalState);
    const listenerRef = useRef<(state: DiscordHubState) => void>(null);

    const {
        gameId,
        jwt
    } = useAppStore.getState();

    useEffect(() => {
        const listener = (newState: DiscordHubState) => setState(newState);
        listenerRef.current = listener;
        globalListeners.add(listener);

        if (globalListeners.size === 1) {
            (async () => {
                await createConnection(jwt);
                if (gameId) {
                    await joinGameGroup(gameId);
                } else {
                    console.warn('Failed to join game signals: no gameId');
                }
            })();
        } else if (gameId && globalConnection?.state === HubConnectionState.Connected) {
            joinGameGroup(gameId).catch(console.error);
        }

        return () => {
            if (listenerRef.current) {
                globalListeners.delete(listenerRef.current);
            }

            // Auto-disconnect when no more listeners
            if (globalListeners.size === 0) {
                setTimeout(() => {
                    if (globalListeners.size === 0 && globalConnection) {
                        globalConnection.stop();
                        globalConnection = null;
                    }
                }, 100);
            }
        };
    }, [gameId, jwt]);

    return state;
};

export const joinGameGroup = async (gameId: string) => {
    if (!globalConnection || globalConnection.state !== HubConnectionState.Connected) {
        return;
    }
    await globalConnection.invoke('JoinGameGroup', gameId);
};

export const leaveGameGroup = async (gameId: string) => {
    if (!globalConnection || globalConnection.state !== HubConnectionState.Connected) {
        return;
    }
    await globalConnection.invoke('LeaveGameGroup', gameId);
};

export const updateDiscordHubState = (updates: Partial<DiscordHubState>) => {
    setState(updates);
};
export const resetDiscordHub = resetDiscordHubState;
