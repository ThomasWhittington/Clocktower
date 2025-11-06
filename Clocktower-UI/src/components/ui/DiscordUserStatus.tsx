import {
    useAppStore
} from "@/store";
import {
    resetAllApplicationState
} from "@/utils";

export const DiscordUserStatus = () => {
    const currentUser = useAppStore((state) => state.currentUser);

    const handleLogout = async () => {
        if (window.confirm("Continue log out?")) {
            resetAllApplicationState();
            window.location.href = '/';
        }
    };

    return (
        <>
            {currentUser &&
                <div
                    className="relative group w-10">
                    <img
                        src={currentUser.avatarUrl}
                        alt={currentUser.name}
                        className="w-full h-full object-cover rounded-lg shadow-md"/>

                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        className="absolute top-1/2 left-1/2 w-8 h-8 transform -translate-x-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 cursor-pointer text-red-600 scale-x-[-1]"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                        onClick={handleLogout}
                    >
                        <path
                            d="M16 13v-2H7V8l-5 4 5 4v-3h9zM20 3H10c-1.1 0-2 .9-2 2v4h2V5h10v14H10v-4H8v4c0 1.1.9 2 2 2h10c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2z"/>
                    </svg>
                </div>
            }
        </>
    );
}