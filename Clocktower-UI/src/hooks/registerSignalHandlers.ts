import type * as signalR from '@microsoft/signalr';
import {
    type AudioEvent,
    DiscordTown,
    GameTime,
    NominationSession,
    Script,
    TalkRequest,
    type TimerState
} from '@/types';
import {useAppStore} from '@/store';

type StateUpdater = (updates: Partial<{
    discordTown?: DiscordTown;
    userPresenceStates: Record<string, boolean>;
    userVoiceStates: Record<string, any>;
    connectionState: signalR.HubConnectionState;
    gameTime: GameTime;
    audioEvent?: AudioEvent;
    timer?: TimerState;
    script?: Script;
    nominationSession?: NominationSession;
    talkRequests: TalkRequest[];
}>) => void;

export const registerSignalHandlers = (
    connection: signalR.HubConnection,
    setState: StateUpdater
) => {
    connection.on('DiscordTownUpdated', (discordTown: DiscordTown) => {
        const {joinedGameId} = useAppStore.getState();
        if (discordTown.gameId !== joinedGameId) return;
        console.log(`🏪 Received DiscordTownUpdated for game ${discordTown.gameId}:`, discordTown);
        setState({discordTown: new DiscordTown(discordTown)});
    });
    connection.on('TownTimeChanged', (gameId: string, gameTime: number) => {
        const {joinedGameId} = useAppStore.getState();
        if (gameId !== joinedGameId) return;
        console.log(`⏰ Received TownTimeChanged for game ${gameId}: ${gameTime}`);
        setState({gameTime: gameTime as GameTime});
    });
    connection.on('TimerUpdated', (timer: TimerState) => {
        const {joinedGameId} = useAppStore.getState();
        if (timer.gameId !== joinedGameId) return;
        console.log(`⏱️ Received TimerUpdated for game ${timer.gameId}:`, timer);
        setState({timer});
    });
    connection.on('ScriptUpdated', (gameId: string, script?: Script) => {
        const {joinedGameId} = useAppStore.getState();
        if (gameId !== joinedGameId) return;
        console.log(`📜 Received ScriptUpdated for game ${gameId}:`, script);
        setState({script: script ? new Script(script) : undefined});
    });

    connection.on('PingUser', (message: string) => {
        const {joinedGameId} = useAppStore.getState();
        console.log(`🏓 Received ping for game ${joinedGameId ?? 'UNKNOWN'}: ${message}`);
    });
    connection.on('NominationUpdate', (gameId: string, session?: NominationSession) => {
        const {joinedGameId} = useAppStore.getState();
        if (gameId !== joinedGameId) return;
        console.log(`🔒 Received NominationUpdate for game ${gameId}:`, session);
        setState({nominationSession: session ? new NominationSession(session) : undefined});
    });
    connection.on('TalkRequestsUpdate', (gameId: string, talkRequests: TalkRequest[]) => {
        const {joinedGameId} = useAppStore.getState();
        if (gameId !== joinedGameId) return;
        console.log(`💬 Received TalkRequestsUpdate for game ${gameId}:`, talkRequests);
        setState({talkRequests: talkRequests ? talkRequests.map(t => new TalkRequest(t)) : []});
    });
    connection.on('PlayAudio', (gameId: string, audioId: number) => {
        const {joinedGameId} = useAppStore.getState();
        if (gameId !== joinedGameId) return;
        console.log(`🔊 Received PlayAudio for game ${gameId}: ${audioId}`);
        setState({audioEvent: {id: crypto.randomUUID(), audioId}});
    });
};