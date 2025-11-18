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
    Unknown = 0,
    Day = 1,
    Evening = 2,
    Night = 3,
}

export type GameStateDto = {
    gameTime: GameTime
};
type DiscordHubState = {
    townOccupancy?: TownOccupants;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime
};

let joinedGameId: string | null = null;
let globalConnection: signalR.HubConnection | null = null;
let globalState: DiscordHubState = {
    userVoiceStates: {},
    connectionState: signalR.HubConnectionState.Disconnected,
    gameTime: GameTime.Night
};
const globalListeners = new Set<(state: DiscordHubState) => void>();

const notifyListeners = () => {
    for (const listener of globalListeners) {
        listener({...globalState});
    }
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

        (async () => {
            await createConnection(jwt);
            if (gameId && globalConnection?.state === HubConnectionState.Connected) {
                await joinGameGroup(gameId);
            } else if (!gameId) {
                console.warn('Failed to join game signals: no gameId');
            }
        })().catch(console.error);

        return () => {
            if (listenerRef.current) {
                globalListeners.delete(listenerRef.current);
            }

            if (globalListeners.size === 0) {
                setTimeout(() => {
                    if (globalListeners.size === 0 && globalConnection) {
                        globalConnection.stop();
                        globalConnection = null;
                        joinedGameId = null;
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

    if (joinedGameId === gameId) {
        return;
    }

    if (joinedGameId && joinedGameId !== gameId) {
        await globalConnection.invoke('LeaveGameGroup', joinedGameId);
    }

    const snapshot = await globalConnection.invoke<GameStateDto | null>('JoinGameGroup', gameId);
    joinedGameId = gameId;
    if (snapshot) {
        setState({
            gameTime: snapshot.gameTime
        });
    }
};

export const leaveGameGroup = async (gameId: string) => {
    if (!globalConnection || globalConnection.state !== HubConnectionState.Connected) {
        return;
    }
    await globalConnection.invoke('LeaveGameGroup', gameId);
    joinedGameId = null;
};

export const updateDiscordHubState = (updates: Partial<DiscordHubState>) => {
    setState(updates);
};
export const resetDiscordHub = resetDiscordHubState;
