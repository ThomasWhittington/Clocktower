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

//TODO move to utils
const convertStringIdsToBigints = (obj: any): any => {
    if (obj === null || typeof obj !== 'object') {
        if (typeof obj === 'string' && /^\d{15,}$/.test(obj)) {
            return BigInt(obj);
        }
        return obj;
    }

    if (Array.isArray(obj)) {
        return obj.map(convertStringIdsToBigints);
    }

    const converted: any = {};
    for (const [key, value] of Object.entries(obj)) {
        converted[key] = convertStringIdsToBigints(value);
    }
    return converted;
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
            const convertedOccupants = convertStringIdsToBigints(occupants) as TownOccupants;
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
