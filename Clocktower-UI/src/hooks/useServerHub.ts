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
import {
    type DiscordTown,
    GameTime,
    TimerStatus,
    type VoiceState
} from '@/types';
import {
    useAppStore
} from "@/store";

type UserPresenceStates = Record<string, boolean>;
type UserVoiceStates = Record<string, VoiceState>;

export type TimerState = {
    gameId: string;
    status: TimerStatus;
    serverNowUtc: string;
    endUtc?: string | null;
    label?: string | null;
};

export type SessionSyncState = {
    gameTime: GameTime,
    jwt: string,
    discordTown?: DiscordTown;
    timer?: TimerState;
};
type HubState = {
    discordTown?: DiscordTown;
    userPresenceStates: UserPresenceStates;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime;
    timer?: TimerState;
};

let joinedGameId: string | null = null;
let globalConnection: signalR.HubConnection | null = null;
let globalState: HubState = {
    userPresenceStates: {},
    userVoiceStates: {},
    connectionState: signalR.HubConnectionState.Disconnected,
    gameTime: GameTime.Night,
    timer: undefined
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

    globalConnection.on('DiscordTownUpdated', (discordTown: DiscordTown) => {
        console.log(`🏪 Received DiscordTownUpdated for game ${joinedGameId}:`, discordTown);
        setState({discordTown: discordTown});
    });

    globalConnection.on('TownTimeChanged', (gameTime: number) => {
        console.log(`⏰ Received TownTimeChanged for game ${joinedGameId}: ${gameTime}`);
        setState({gameTime: gameTime as GameTime});
    });

    globalConnection.on('TimerUpdated', (timer: TimerState) => {
        console.log(`⏱️ Received TimerUpdated for game ${joinedGameId}:`, timer);
        setState({timer});
    });

    globalConnection.on('PingUser', (message: string) => {
        console.log(`🏓 Received ping for game ${joinedGameId}: ${message}`);
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
    const lastGameIdRef = useRef<string | null>(null);
    const {gameId} = useAppStore.getState();

    useEffect(() => {
        const listener = (newState: HubState) => setState(newState);
        listenerRef.current = listener;
        globalListeners.add(listener);

        if (!initializedRef.current) {
            initializedRef.current = true;
            lastGameIdRef.current = gameId;

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
                        joinPromise = null;
                    }
                }, 100);
            }
        };
    }, []);

    useEffect(() => {
        if (initializedRef.current && isConnected(globalConnection) && gameId !== lastGameIdRef.current) {
            console.log(`Game ID changed from ${lastGameIdRef.current} to ${gameId}`);
            lastGameIdRef.current = gameId;

            if (gameId) {
                joinGameGroup(gameId).catch(console.error);
            }
        }
    }, [gameId]);
    return state;
};

let joinPromise: Promise<void> | null = null;

export const joinGameGroup = async (gameId: string): Promise<void> => {

    if (joinPromise) {
        await joinPromise;
        if (joinedGameId === gameId) {
            return;
        }
    }

    const {
        setJwt,
        currentUser
    } = useAppStore.getState();

    if (!isConnected(globalConnection)) {
        return;
    }

    joinPromise = (async () => {
        try {
            if (joinedGameId && joinedGameId !== gameId) {
                console.log(`Leaving game : ${joinedGameId}`);
                try {
                    await globalConnection.invoke('LeaveGameGroup', joinedGameId);
                    console.log(`Successfully left game : ${joinedGameId}`);
                } catch (error) {
                    console.error(`Failed to leave game ${joinedGameId}:`, error);
                }
            }

            console.log(`Calling join game : ${gameId}`);
            const snapshot = await globalConnection.invoke<SessionSyncState | null>('JoinGameGroup', gameId, currentUser?.id);
            joinedGameId = gameId;
            console.log(`Successfully joined game : ${gameId}`);

            if (snapshot) {
                setState({
                    gameTime: snapshot.gameTime,
                    discordTown: snapshot.discordTown,
                    timer: snapshot.timer
                });

                const currentJwt = useAppStore.getState().jwt;
                if (snapshot.jwt !== currentJwt) {
                    const jwtChanged = hasJwtMeaningfullyChanged(currentJwtContent, snapshot.jwt);
                    setJwt(snapshot.jwt);

                    if (jwtChanged) {
                        console.log('JWT content changed, reconnecting...');
                        currentJwtContent = snapshot.jwt;
                        joinPromise = null;
                        await handleJwtUpdate();
                        return;
                    } else {
                        console.log('JWT expiration updated, no reconnection needed');
                        currentJwtContent = snapshot.jwt;
                    }
                }
            }
        } catch (error) {
            console.error(`Failed to join game ${gameId}:`, error);
            joinedGameId = null;
            throw error;
        }
    })();

    try {
        await joinPromise;
    } finally {
        joinPromise = null;
    }
};

export const leaveGameGroup = async (gameId: string) => {
    if (!globalConnection || globalConnection.state !== HubConnectionState.Connected) {
        return;
    }
    await globalConnection.invoke('LeaveGameGroup', gameId);
    joinedGameId = null;
};
