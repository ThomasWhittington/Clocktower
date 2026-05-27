import {
    useEffect,
    useRef,
    useState
} from 'react';
import * as signalR from '@microsoft/signalr';
import {HubConnectionState} from '@microsoft/signalr';
import {
    type AudioEvent,
    DiscordTown,
    GameTime,
    NominationSession,
    Script,
    type SessionSyncState,
    TalkRequest,
    type TimerState,
    type VoiceState
} from '@/types';
import {useAppStore} from "@/store";
import {registerSignalHandlers} from "@/hooks/registerSignalHandlers.ts";
import {VoteHistoryRecord} from "@/types/voteHistoryRecord.ts";

type UserPresenceStates = Record<string, boolean>;
type UserVoiceStates = Record<string, VoiceState>;

type HubState = {
    discordTown?: DiscordTown;
    userPresenceStates: UserPresenceStates;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime;
    audioEvent?: AudioEvent;
    timer?: TimerState;
    script?: Script;
    nominationSession?: NominationSession;
    talkRequests: TalkRequest[];
};

let globalConnection: signalR.HubConnection | null = null;
let globalState: HubState = {
    userPresenceStates: {},
    userVoiceStates: {},
    connectionState: signalR.HubConnectionState.Disconnected,
    gameTime: GameTime.Night,
    timer: undefined,
    script: undefined,
    nominationSession: undefined,
    talkRequests: []
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
        gameTime: GameTime.Night,
        audioEvent: undefined,  
        talkRequests: []
    };
    notifyListeners();
};

let currentJwtContent: string | null = null;
let isInitialized = false;
let isHandlingJwtUpdate = false;

const handleJoinSnapshot = async (snapshot: SessionSyncState, isReconnecting: boolean) => {
    const {setJwt} = useAppStore.getState();
    setState({
        gameTime: snapshot.gameTime,
        discordTown: snapshot.discordTown ? new DiscordTown(snapshot.discordTown) : undefined,
        timer: snapshot.timer,
        script: snapshot.script ? new Script(snapshot.script) : undefined,
        nominationSession: snapshot.nominationSession ? new NominationSession(snapshot.nominationSession) : undefined,
        talkRequests: snapshot.talkRequests ? snapshot.talkRequests.map((req) => new TalkRequest(req)) : [],
    });

    const currentJwt = useAppStore.getState().jwt;
    if (snapshot.jwt !== currentJwt) {
        const jwtChanged = hasJwtMeaningfullyChanged(currentJwtContent, snapshot.jwt);
        currentJwtContent = snapshot.jwt;
        setJwt(snapshot.jwt);

        if (jwtChanged && !isReconnecting) {
            console.log('JWT content changed from server snapshot');
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
            transport: signalR.HttpTransportType.WebSockets,
            skipNegotiation: true,
            accessTokenFactory: () => {
                const {jwt} = useAppStore.getState();
                return jwt ?? '';
            }
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    registerSignalHandlers(globalConnection, setState);

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
    if (isHandlingJwtUpdate) {
        console.log('JWT update already in progress, skipping...');
        return;
    }

    if (isConnected(globalConnection)) {
        isHandlingJwtUpdate = true;
        try {
            const {gameId, joinedGameId} = useAppStore.getState();
            const targetGameId = gameId || joinedGameId;

            await globalConnection.stop();
            globalConnection = null;

            await createConnection();

            if (targetGameId && isConnected(globalConnection)) {
                await joinGameGroup(targetGameId, true);
            }
        } finally {
            isHandlingJwtUpdate = false;
        }
    }
};

const hasJwtMeaningfullyChanged = (oldJwt: string | null, newJwt: string): boolean => {
    if (!oldJwt) return true;

    try {
        const oldPayload = JSON.parse(atob(oldJwt.split('.')[1]));
        const newPayload = JSON.parse(atob(newJwt.split('.')[1]));

        const {exp: oldExp, ...oldRest} = oldPayload;
        const {exp: newExp, ...newRest} = newPayload;

        return JSON.stringify(oldRest) !== JSON.stringify(newRest);
    } catch {
        return true;
    }
};

let jwtUnsubscribe: (() => void) | null = null;

const setupGlobalJwtMonitoring = () => {
    if (jwtUnsubscribe) return;

    jwtUnsubscribe = useAppStore.subscribe((state, prevState) => {
        if (!isInitialized || isHandlingJwtUpdate) return;

        const jwt = state.jwt;
        const prevJwt = prevState.jwt;

        if (jwt === prevJwt || !jwt) return;

        const jwtChanged = hasJwtMeaningfullyChanged(currentJwtContent, jwt);

        if (jwtChanged) {
            console.log('JWT content changed, triggering reconnect...');
            currentJwtContent = jwt;
            handleJwtUpdate().catch(console.error);
        }
    });
};

export const useServerHub = () => {
    const [state, setState] = useState<HubState>(globalState);
    const listenerRef = useRef<((state: HubState) => void) | null>(null);
    const {gameId} = useAppStore();
    const lastGameIdRef = useRef<string | null>(gameId);

    useEffect(() => {
        const listener = (newState: HubState) => setState(newState);
        listenerRef.current = listener;
        globalListeners.add(listener);

        if (!isInitialized) {
            isInitialized = true;

            setupGlobalJwtMonitoring();

            (async () => {
                await createConnection();

                const {gameId: currentGameId, jwt: currentJwt} = useAppStore.getState();

                if (currentJwt) {
                    currentJwtContent = currentJwt;
                }

                if (currentGameId && isConnected(globalConnection)) {
                    await joinGameGroup(currentGameId, false, true);
                } else if (!currentGameId) {
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
        if (!isInitialized) return;

        if (isConnected(globalConnection) && gameId !== lastGameIdRef.current) {
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


const openNominations = async (gameId: string): Promise<void> => {
    if (!isConnected(globalConnection)) {
        return;
    }

    await globalConnection.invoke('OpenNominations', gameId);
}
const closeNominations = async (gameId: string): Promise<void> => {
    if (!isConnected(globalConnection)) {
        return;
    }

    await globalConnection.invoke('CloseNominations', gameId);
}

const nextNomination = async (gameId: string): Promise<void> => {
    if (!isConnected(globalConnection)) {
        return;
    }

    await globalConnection.invoke('NextNomination', gameId);
}
const startVote = async (gameId: string, votingSpeed: number): Promise<void> => {
    if (!isConnected(globalConnection)) {
        return;
    }

    await globalConnection.invoke('StartVote', gameId, votingSpeed);
}
const cancelVote = async (gameId: string): Promise<void> => {
    if (!isConnected(globalConnection)) {
        return;
    }

    await globalConnection.invoke('CancelVote', gameId);
}

const makeNomination = async (gameId: string, nominatorId: string, nomineeId: string) => {
    if (!isConnected(globalConnection)) {
        return;
    }
    return await globalConnection.invoke<boolean | null>('MakeNomination', gameId, nominatorId, nomineeId);
}

const toggleVote = async (gameId: string, playerId: string) => {
    if (!isConnected(globalConnection)) {
        return;
    }
    return await globalConnection.invoke<boolean | null>('ToggleVote', gameId, playerId);
}
const toggleMarkPlayer = async (gameId: string, playerId: string) => {
    if (!isConnected(globalConnection)) {
        return;
    }
    return await globalConnection.invoke<boolean | null>('ToggleMarkPlayer', gameId, playerId);
}

const removeAllMarks = async (gameId: string) => {
    if (!isConnected(globalConnection)) {
        return;
    }
    return await globalConnection.invoke<boolean | null>('RemoveAllMarks', gameId);
}

const getVoteHistory = async (gameId: string) => {
    if (!isConnected(globalConnection)) {
        return;
    }
    const records = await globalConnection.invoke<VoteHistoryRecord[] | null>('GetVoteHistory', gameId);
    return records?.map(record => new VoteHistoryRecord(record)) ?? null;
}

const requestToTalk = async (gameId: string, requesterId: string, targetId: string) => {
    if (!isConnected(globalConnection)) {
        return;
    }

    await globalConnection.invoke('RequestToTalk', gameId, requesterId, targetId);
};
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
            script: globalState.script,
            userPresenceStates: globalState.userPresenceStates,
            userVoiceStates: globalState.userVoiceStates
        };
        try {
            const {joinedGameId: latestId} = useAppStore.getState();


            setState({
                discordTown: undefined,
                timer: undefined,
                script: undefined,
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
export {
    openNominations,
    closeNominations,
    startVote,
    cancelVote,
    nextNomination,
    makeNomination,
    toggleVote,
    toggleMarkPlayer,
    getVoteHistory,
    requestToTalk,
    removeAllMarks
};
