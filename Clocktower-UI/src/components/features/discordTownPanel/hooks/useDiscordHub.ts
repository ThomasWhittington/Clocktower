import {
    useEffect,
    useState
} from 'react';
import * as signalR
    from '@microsoft/signalr';
import type {
    TownOccupants
} from "../../../../types";
import {
    ConverterUtils
} from "../../../../utils";


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

        connection.on('TownOccupancyUpdated', (occupants: TownOccupants) => {
            const convertedOccupants = ConverterUtils.convertStringIdsToBigints(occupants) as TownOccupants;
            setState(prev => ({
                ...prev,
                townOccupancy: convertedOccupants,
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
