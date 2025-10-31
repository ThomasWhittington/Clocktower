import {
    useEffect,
    useState
} from 'react';
import * as signalR
    from '@microsoft/signalr';
import type {
    TownOccupants
} from "../../../../types";


type DiscordHubState = {
    townOccupancy?: TownOccupants;
};

export const useDiscordHub = (): DiscordHubState => {
    const [state, setState] = useState<DiscordHubState>({});

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5120/discordHub')
            .withAutomaticReconnect()
            .build();
        console.log(connection)

        connection.on('TownOccupancyUpdated', (occupants: TownOccupants) => {
            setState(prev => ({
                ...prev,
                townOccupancy: occupants,
                lastUpdate: 'TownOccupancyUpdated'
            }));
        });

        connection.start().catch(console.error);

        return () => {
            connection.stop();
        };
    }, []);

    return state;
};
