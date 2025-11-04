import {
    useEffect,
    useState
} from 'react';
import {
    useAppStore
} from '@/store';
import {
    useDiscordHub
} from './useDiscordHub';
import {
    discordService
} from '@/services';
import {
    ValidationUtils
} from '@/utils';
import type {
    TownOccupants
} from '@/types';

export const useTownOccupancy = () => {
    const [townOccupancy, setTownOccupancy] = useState<TownOccupants>();
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string>("");

    const guildId = useAppStore((state) => state.guildId);
    const {townOccupancy: realtimeTownOccupancy} = useDiscordHub();

    useEffect(() => {
        const getTownOccupancy = async () => {
            if (!ValidationUtils.isValidDiscordId(guildId)) {
                console.error('guildId was not valid');
                return;
            }

            setIsLoading(true);
            setError("");

            try {
                const data = await discordService.getTownOccupancy(guildId);
                setTownOccupancy(data);
            } catch (err: any) {
                setError(err.message);
            } finally {
                setIsLoading(false);
            }
        };

        getTownOccupancy();
    }, [guildId]);

    useEffect(() => {
        if (realtimeTownOccupancy) {
            setTownOccupancy(realtimeTownOccupancy);
        }
    }, [realtimeTownOccupancy]);

    return {
        townOccupancy,
        isLoading,
        error
    };
};