import {BasePanel} from "@/components/ui";
import {AddUsers} from "./AddUsers";
import {UserManager} from "./UserManager";
import {useUserControls} from "@/components/features/gameWindow/hooks";
import {useEffect} from "react";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";

interface UserManagerPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const UserManagerPanel = ({isOpen, onClose}: UserManagerPanelProps) => {
    const {discordTown} = useDiscordTown();
    const {
        availableUsers,
        getAvailableGameUsers,
        addUserToGame,
        inviteAll,
        inviteUser,
        changeUserType,
        removeUser,
        randomiseSeatingPositions,
        isLoading,
        canRun
    } = useUserControls();

    useEffect(() => {
        if (isOpen) {
            void getAvailableGameUsers();
        }
    }, [isOpen, discordTown, getAvailableGameUsers]);

    return (
        <BasePanel title="User Manager" isOpen={isOpen} onClose={onClose} className="user-manager">
            <UserManager
                inviteAll={inviteAll}
                inviteUser={inviteUser}
                changeUserType={changeUserType}
                removeUser={removeUser}
                randomiseSeatingPositions={randomiseSeatingPositions}
                isLoading={isLoading}
                canRun={canRun}
                discordTown={discordTown}
            />
            <AddUsers
                availableUsers={availableUsers}
                addUserToGame={addUserToGame}
            />
        </BasePanel>
    )
};



