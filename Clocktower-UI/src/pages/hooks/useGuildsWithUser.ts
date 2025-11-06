import {
    useCallback,
    useEffect,
    useState
} from 'react';
import type {
    MiniGuild
} from "@/types";
import {
    discordService
} from "@/services";
import {
    useAppStore
} from "@/store";

export const useGuildsWithUser = (enabled: boolean = true) => {
    const [guilds, setGuilds] = useState<MiniGuild[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const currentUser = useAppStore((state) => state.currentUser);

    const fetchGuilds = useCallback(async (cancelledRef?: { current: boolean }) => {
        if (!enabled || !currentUser) {
            setLoading(false);
            return;
        }

        try {
            setLoading(true);
            setError(null);
            const data = await discordService.getGuildsWithUser(currentUser.id);

            if (!cancelledRef?.current) {
                setGuilds(data);
            }
        } catch (err) {
            if (!cancelledRef?.current) {
                setError(err instanceof Error ? err.message : 'Failed to fetch guilds');
            }
        } finally {
            if (!cancelledRef?.current) {
                setLoading(false);
            }
        }
    }, [enabled, currentUser]);

    useEffect(() => {
        const cancelledRef = { current: false };

        fetchGuilds(cancelledRef);

        return () => {
            cancelledRef.current = true;
        };
    }, [fetchGuilds]);

    const refetch = () => fetchGuilds();

    return {
        guilds,
        loading,
        error,
        refetch
    };
};