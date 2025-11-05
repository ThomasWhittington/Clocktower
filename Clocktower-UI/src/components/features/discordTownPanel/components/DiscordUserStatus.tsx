import {
    useAppStore
} from "@/store";
import {
    authService
} from "@/services";

import {
    useAddBot
} from '@/components/features/discordTownPanel/hooks';
import {
    resetAllApplicationState
} from "@/utils";

function DiscordUserStatus() {
    const currentUser = useAppStore((state) => state.currentUser);
    const {addBot} = useAddBot();

    const handleLogout = () => {
        resetAllApplicationState();
        window.location.href = '/';
    };
    return (
        <>
            {currentUser ? (
                <div
                    className="flex items-center space-x-2">
                    <p className="text-purple-700">Logged in as {currentUser.name}</p>
                    <button
                        onClick={addBot}
                        className="bg-red-500 text-white px-4 py-2 rounded-2xl hover:bg-red-600"
                    >
                        Add Bot To Server
                    </button>
                    <button
                        onClick={() => handleLogout()}
                        className="bg-red-500 text-white px-4 py-2 rounded-2xl hover:bg-red-600"
                    >
                        Logout
                    </button>
                </div>
            ) : (
                <button
                    onClick={() => authService.initiateDiscordLogin()}
                    className="bg-blue-500 text-white px-4 py-2 rounded-2xl hover:bg-blue-600"
                >
                    Login with Discord
                </button>
            )}
        </>
    );
}

export default DiscordUserStatus;