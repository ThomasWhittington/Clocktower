import {
    useEffect
} from 'react';

import {
    useAppStore
} from "@/store";
import {
    Spinner
} from "@/components/ui";

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
            className="loading flex items-center justify-center min-h-screen">
            <div
                className="text-center">
                <Spinner
                    className="mx-auto justify-items-center"/>
                <p>Processing authentication...</p>
            </div>
        </div>
    );
};

export default BotCallback;