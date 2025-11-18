import {
    useAppStore
} from "@/store";
import {
    useEffect
} from "react";
import {
    discordService
} from "@/services";
import {
    resetAllApplicationState
} from "@/utils";
import {
    Spinner
} from "@/components/ui";

const Join = () => {
    const {
        setJwt,
        setCurrentUser,
        setGuildId,
        setGameId
    } = useAppStore.getState();

    useEffect(() => {
        const handleJoin = async () => {
            resetAllApplicationState();
            const urlParams = new URLSearchParams(globalThis.location.search);
            const key = urlParams.get('key');
            const urlError = urlParams.get('error');
            if (urlError) {
                globalThis.location.href = '/error?error=' + encodeURIComponent(urlError);
                return;
            }
            if (key) {
                try {
                    const {
                        data,
                        error
                    } = await discordService.getJoinData(key);

                    if (error) {
                        console.error('Failed to get join data:', error);
                        globalThis.location.href = '/error?error=join_data_failed';
                    } else if (data?.user && data.guildId && data.gameId) {
                        setCurrentUser({
                            id: data.user?.id ?? '',
                            name: data.user?.name ?? '',
                            avatarUrl: data.user?.avatarUrl ?? ''
                        });
                        setGuildId(data.guildId);
                        setGameId(data.gameId);
                        setJwt(data?.jwt ?? undefined);

                        globalThis.location.href = '/game';
                    } else {
                        console.error('Unexpected join data:', data);
                        globalThis.location.href = '/error?error=join_data_failed';
                    }
                } catch (error) {
                    console.error('Failed to get join data:', error);
                    globalThis.location.href = '/error?error=join_data_failed';
                }
            } else {
                globalThis.location.href = '/error?error=missing_key';
            }
        };
        handleJoin().then(_ => {
        });
    }, []);

    return (
        <div
            className="loading flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <Spinner
                    className="mx-auto justify-items-center"/>
                <p>Joining game...</p>
            </div>
        </div>
    );
}
export default Join;