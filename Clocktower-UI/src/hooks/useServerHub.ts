import {useEffect, useRef, useState} from 'react';
import * as signalR from '@microsoft/signalr';
import {HubConnectionState} from '@microsoft/signalr';
import {DiscordTown, GameTime, type SessionSyncState, type TimerState, type VoiceState} from '@/types';
import {useAppStore} from "@/store";

type UserPresenceStates = Record<string, boolean>;
type UserVoiceStates = Record<string, VoiceState>;

type HubState = {
    discordTown?: DiscordTown;
    userPresenceStates: UserPresenceStates;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime;
    timer?: TimerState;
};

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

let currentJwtContent: string | null = null;
const handleJoinSnapshot = async (snapshot: SessionSyncState, isReconnecting: boolean) => {
    const {setJwt} = useAppStore.getState();
    setState({
        gameTime: snapshot.gameTime,
        discordTown: snapshot.discordTown ? new DiscordTown(snapshot.discordTown) : undefined,
        timer: snapshot.timer
    });

    const currentJwt = useAppStore.getState().jwt;
    if (snapshot.jwt !== currentJwt) {
        const jwtChanged = hasJwtMeaningfullyChanged(currentJwtContent, snapshot.jwt);
        setJwt(snapshot.jwt);
        currentJwtContent = snapshot.jwt;

        if (jwtChanged && !isReconnecting) {
            console.log('JWT content changed, reconnecting...');
            joinPromise = null;
            await handleJwtUpdate();
            return;
        }
    }
}
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
        const {joinedGameId} = useAppStore.getState();
        if (discordTown.gameId !== joinedGameId) return;
        console.log(`🏪 Received DiscordTownUpdated for game ${discordTown.gameId}:`, discordTown);
        setState({discordTown: new DiscordTown(discordTown)});
    });

    globalConnection.on('TownTimeChanged', (gameId: string, gameTime: number) => {
        const {joinedGameId} = useAppStore.getState();
        if (gameId !== joinedGameId) return;
        console.log(`⏰ Received TownTimeChanged for game ${gameId}: ${gameTime}`);
        setState({gameTime: gameTime as GameTime});
    });

    globalConnection.on('TimerUpdated', (timer: TimerState) => {
        const {joinedGameId} = useAppStore.getState();
        if (timer.gameId !== joinedGameId) return;
        console.log(`⏱️ Received TimerUpdated for game ${timer.gameId}:`, timer);
        setState({timer});
    });

    globalConnection.on('PingUser', (message: string) => {
        const {joinedGameId} = useAppStore.getState();
        console.log(`🏓 Received ping for game ${joinedGameId ?? 'UNKNOWN'}: ${message}`);
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
        const {gameId, joinedGameId} = useAppStore.getState();
        const targetGameId = gameId || joinedGameId;

        await globalConnection.stop();
        globalConnection = null;

        await createConnection();

        if (targetGameId && isConnected(globalConnection)) {
            await joinGameGroup(targetGameId, true);
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
                    await joinGameGroup(gameId, false, true);
                } else if (!gameId) {
                    console.warn('Failed to join game signals: no gameId');
                }
            })().catch(console.error);
        }

        return () => {
            if (listenerRef.current) {
                globalListeners.delete(listenerRef.current);
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

export const joinGameGroup = async (gameId: string, isReconnecting: boolean = false, isInitialMount: boolean = false): Promise<void> => {
    const {setJoinedGameId, currentUser} = useAppStore.getState();

    if (joinPromise) {
        await joinPromise;
    }
    if (!currentUser) {
        console.error('Cannot join game: user not authenticated');
        return;
    }
    const currentJoinedId = useAppStore.getState().joinedGameId;

    if (currentJoinedId === gameId && isConnected(globalConnection) && !isReconnecting && !isInitialMount) {
        console.log(`Already in game ${gameId}, skipping join.`);
        return;
    }

    if (!isConnected(globalConnection)) {
        return;
    }
    joinPromise = (async () => {
        const previousState = {
            discordTown: globalState.discordTown,
            timer: globalState.timer,
            userPresenceStates: globalState.userPresenceStates,
            userVoiceStates: globalState.userVoiceStates
        };
        try {
            const {joinedGameId: latestId} = useAppStore.getState();


            setState({
                discordTown: undefined,
                timer: undefined,
                userPresenceStates: {},
                userVoiceStates: {}
            });
            console.log(`Calling join game : ${gameId} (leaving ${latestId})`);
            const snapshot = await globalConnection.invoke<SessionSyncState | null>(
                'JoinGameGroup',
                gameId,
                currentUser.id,
                latestId
            );

            setJoinedGameId(gameId);

            console.log(`Successfully joined game : ${gameId}`);

            if (snapshot) {
                await handleJoinSnapshot(snapshot, isReconnecting);
            }
        } catch (error) {
            console.error(`Failed to join game ${gameId}:`, error);
            setJoinedGameId(null);
            setState(previousState);
            throw error;
        }
    })();

    try {
        await joinPromise;
    } finally {
        joinPromise = null;
    }
};
