import {
    useEffect,
    useRef,
    useState
} from 'react';
import * as signalR
    from '@microsoft/signalr';
import type {
    TownOccupants
} from '../../../../types';

type UserVoiceStates = Record<string, boolean>;

type DiscordHubState = {
    townOccupancy?: TownOccupants;
    userVoiceStates: UserVoiceStates;
    connectionState: signalR.HubConnectionState;
};

let globalConnection: signalR.HubConnection | null = null;
let globalState: DiscordHubState = {
    userVoiceStates: {},
    connectionState: signalR.HubConnectionState.Disconnected
};
const globalListeners = new Set<(state: DiscordHubState) => void>();

const notifyListeners = () => {
    globalListeners.forEach(listener => listener({ ...globalState }));
};

const setState = (updates: Partial<DiscordHubState>) => {
    globalState = { ...globalState, ...updates };
    notifyListeners();
};

const createConnection = async () => {
    if (globalConnection) return;

    globalConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5120/discordHub')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    globalConnection.on('TownOccupancyUpdated', (occupants: TownOccupants) => {
        setState({ townOccupancy: occupants });
    });

    globalConnection.on('UserVoiceStateChanged', (userId: string, isInVoice: boolean) => {
        setState({
            userVoiceStates: { ...globalState.userVoiceStates, [userId]: isInVoice }
        });
    });

    globalConnection.onclose(() => setState({ connectionState: signalR.HubConnectionState.Disconnected }));
    globalConnection.onreconnecting(() => setState({ connectionState: signalR.HubConnectionState.Reconnecting }));
    globalConnection.onreconnected(() => setState({ connectionState: signalR.HubConnectionState.Connected }));

    try {
        await globalConnection.start();
        setState({ connectionState: signalR.HubConnectionState.Connected });
    } catch (error) {
        console.error('SignalR connection failed:', error);
        setState({ connectionState: signalR.HubConnectionState.Disconnected });
    }
};

export const useDiscordHub = () => {
    const [state, setState] = useState<DiscordHubState>(globalState);
    const listenerRef = useRef<(state: DiscordHubState) => void>(null);

    useEffect(() => {
        const listener = (newState: DiscordHubState) => setState(newState);
        listenerRef.current = listener;
        globalListeners.add(listener);

        // Auto-connect on first subscription
        if (globalListeners.size === 1) {
            createConnection();
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
    }, []);

    return state;
};