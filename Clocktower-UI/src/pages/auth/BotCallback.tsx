import {
    useEffect
} from 'react';

import {
    useAppStore
} from "@/store";

const BotCallback = () => {
    const setGuildId = useAppStore((state) => state.setGuildId);

    useEffect(() => {
        const handleAuthCallback = async () => {
            const urlParams = new URLSearchParams(window.location.search);
            const guildId = urlParams.get('guild_id');
            const error = urlParams.get('error');

            if (error) {
                window.location.href = '/login?error=' + encodeURIComponent(error);
                return;
            }

            if (guildId) {
                setGuildId(guildId);
                window.location.href = '/';
            } else {
                   window.location.href = '/login?error=missing_guild';
            }
        };

        handleAuthCallback().then(_ => {
        });
    }, []);

    return (
        <div
            className="flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <div
                    className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto mb-4"></div>
                <p>Processing authentication...</p>
            </div>
        </div>
    );
};

export default BotCallback;