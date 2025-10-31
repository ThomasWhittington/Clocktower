import {
    useEffect,
    useState
} from 'react';
import * as signalR
    from '@microsoft/signalr';
import type {
    TownOccupants
} from "../../../../types";

export const useStateHub = () => {
    const [townOccupancy, setTownOccupancy] = useState<TownOccupants>();

    useEffect(() => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5120/stateHub')
            .withAutomaticReconnect()
            .build();
        console.log(connection)

        connection.on('UserMovedChannel', (occupants: TownOccupants) => {
            setTownOccupancy(occupants);
        });

        connection.start().catch(console.error);

        return () => {
            connection.stop();
        };
    }, []);

    return townOccupancy;
};
