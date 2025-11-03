import {
    useAppStore
} from "../../../../store.ts";
import {
    authService
} from "../../../../services/";

function DiscordUserStatus() {
    const currentUser = useAppStore((state) => state.currentUser);
    const setCurrentUser = useAppStore((state) => state.setCurrentUser);

    const handleLogout = () => {
        setCurrentUser(null!);
    };

    return (
        <>
            {currentUser ? (
                <div
                    className="flex items-center space-x-2">
                    <p className="text-purple-700">Logged in as {currentUser.name}</p>
                    <button
                        onClick={handleLogout}
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