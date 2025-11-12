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

const Join = () => {
    const setCurrentUser = useAppStore((state) => state.setCurrentUser);
    const setGuildId = useAppStore((state) => state.setGuildId);

    useEffect(() => {
        const handleJoin = async () => {
            resetAllApplicationState();
            const urlParams = new URLSearchParams(window.location.search);
            const key = urlParams.get('key');
            const error = urlParams.get('error');

            if (error) {
                window.location.href = '/error?error=' + encodeURIComponent(error);
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
                        window.location.href = '/error?error=join_data_failed';
                    } else {
                        if (data && data.user && data.guildId) {
                            setCurrentUser({
                                id: data.user?.id ?? '',
                                name: data.user?.name ?? '',
                                avatarUrl: data.user?.avatarUrl ?? ''
                            });
                            setGuildId(data.guildId);

                            window.location.href = '/game';
                        } else {
                            console.error('Unexpected join data:', data);
                            window.location.href = '/error?error=join_data_failed';
                        }
                    }
                } catch (error) {
                    console.error('Failed to get join data:', error);
                    window.location.href = '/error?error=join_data_failed';
                }
            } else {
                window.location.href = '/error?error=missing_key';
            }
        };
        handleJoin().then(_ => {
        });
    }, []);

    return (
        <div
            className="flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <div
                    className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto mb-4"></div>
                <p>Joining game...</p>
            </div>
        </div>
    );
}
export default Join;