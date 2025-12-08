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
import type {
    VoiceState
} from "@/types/voiceState.ts";

type UserPresenceStates = Record<string, boolean>;
type UserVoiceStates = Record<string, VoiceState>;

export enum GameTime {
    Unknown = 0,
    Day = 1,
    Evening = 2,
    Night = 3,
}

export type SessionSyncState = {
    gameTime: GameTime,
    jwt: string,
    townOccupancy?: TownOccupants;
};
type HubState = {
    townOccupancy?: TownOccupants;
    userPresenceStates: UserPresenceStates;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime
};

let joinedGameId: string | null = null;
let globalConnection: signalR.HubConnection | null = null;
let globalState: HubState = {
    userPresenceStates: {},
    userVoiceStates: {},
    connectionState: signalR.HubConnectionState.Disconnected,
    gameTime: GameTime.Night
};
const globalListeners = new Set<(state: HubState) => void>();

const notifyListeners = () => {
    for (const listener of globalListeners) {
        listener({...globalState});
    }
};

const setState = (updates: Partial<HubState>) => {
    globalState = {...globalState, ...updates};
    notifyListeners();
};

export const resetHubState = () => {
    globalState = {
        userPresenceStates: {},
        userVoiceStates: {},
        connectionState: HubConnectionState.Disconnected,
        gameTime: GameTime.Night
    };
    notifyListeners();
};
const isConnected = (connection: signalR.HubConnection | null): connection is signalR.HubConnection => {
    return connection !== null && connection.state === HubConnectionState.Connected;
};

const createConnection = async () => {
    if (globalConnection) return;

    globalConnection = new signalR.HubConnectionBuilder()
        .withUrl(import.meta.env.VITE_CLOCKTOWER_SERVER_URI + '/serverHub', {
            accessTokenFactory: () => {
                const {jwt} = useAppStore.getState();
                return jwt ?? '';
            }
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

export const handleJwtUpdate = async () => {
    if (isConnected(globalConnection)) {
        await globalConnection.stop();
        globalConnection = null;
        joinedGameId = null;

        await createConnection();

        const {gameId} = useAppStore.getState();
        if (gameId && isConnected(globalConnection)) {
            await joinGameGroup(gameId);
        }
    }
};

const hasJwtMeaningfullyChanged = (oldJwt: string | null, newJwt: string): boolean => {
    if (!oldJwt) return true;

    try {
        const oldPayload = JSON.parse(atob(oldJwt.split('.')[1]));
        const newPayload = JSON.parse(atob(newJwt.split('.')[1]));

        const {
            exp: oldExp,
            ...oldRest
        } = oldPayload;
        const {
            exp: newExp,
            ...newRest
        } = newPayload;

        return JSON.stringify(oldRest) !== JSON.stringify(newRest);
    } catch {
        return true;
    }
};
let currentJwtContent: string | null = null;

export const useServerHub = () => {
    const [state, setState] = useState<HubState>(globalState);
    const listenerRef = useRef<(state: HubState) => void>(null);
    const initializedRef = useRef(false);
    const {gameId} = useAppStore.getState();

    useEffect(() => {
        const listener = (newState: HubState) => setState(newState);
        listenerRef.current = listener;
        globalListeners.add(listener);

        if (!initializedRef.current) {
            initializedRef.current = true;

            (async () => {
                await createConnection();

                if (gameId && isConnected(globalConnection)) {
                    await joinGameGroup(gameId);
                } else if (!gameId) {
                    console.warn('Failed to join game signals: no gameId');
                }
            })().catch(console.error);
        }

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
                        initializedRef.current = false;
                    }
                }, 100);
            }
        };
    }, []);

    useEffect(() => {
        if (initializedRef.current && isConnected(globalConnection)) {
            if (gameId) {
                joinGameGroup(gameId).catch(console.error);
            }
        }
    }, [gameId]);
    return state;
};

export const joinGameGroup = async (gameId: string) => {
    const {
        setJwt,
        currentUser
    } = useAppStore.getState();

    if (!isConnected(globalConnection)) return;
    if (joinedGameId === gameId) return;

    if (joinedGameId && joinedGameId !== gameId) {
        await globalConnection.invoke('LeaveGameGroup', joinedGameId);
    }

    const snapshot = await globalConnection.invoke<SessionSyncState | null>('JoinGameGroup', gameId, currentUser?.id);
    joinedGameId = gameId;
    if (snapshot) {
        setState({
            gameTime: snapshot.gameTime,
            townOccupancy: snapshot.townOccupancy
        });
        const currentJwt = useAppStore.getState().jwt;
        if (snapshot.jwt !== currentJwt) {
            setJwt(snapshot.jwt);
            if (hasJwtMeaningfullyChanged(currentJwtContent, snapshot.jwt)) {
                console.log('JWT content changed, reconnecting...');
                currentJwtContent = snapshot.jwt;
                await handleJwtUpdate();
            } else {
                console.log('JWT expiration updated, no reconnection needed');
            }
        }
    }
};

export const leaveGameGroup = async (gameId: string) => {
    if (!globalConnection || globalConnection.state !== HubConnectionState.Connected) {
        return;
    }
    await globalConnection.invoke('LeaveGameGroup', gameId);
    joinedGameId = null;
};
