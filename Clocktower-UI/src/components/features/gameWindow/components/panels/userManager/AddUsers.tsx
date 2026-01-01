import {useEffect} from "react";
import {useUserControls} from "@/components/features/gameWindow/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {UserAvatar} from "@/components/ui";

export const AddUsers = () => {
    const {discordTown} = useDiscordTown();
    const {
        availableUsers,
        getAvailableGameUsers,
        addUserToGame,
    } = useUserControls();

    useEffect(() => {
        void getAvailableGameUsers();
    }, [discordTown, getAvailableGameUsers]);

    return (
        <div className="column add-users-view">
            <h3 className="title">Available Users</h3>
            {availableUsers?.map(user =>
                <button key={user.id} className="available-user" onClick={() => addUserToGame(user)}>
                    <UserAvatar user={user} size={48} className="discord-user-avatar"/>
                    <p>{user.name}</p>
                </button>
            )}
        </div>
    );
}
