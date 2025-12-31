import {BasePanel, IconButton, Spinner} from "@/components/ui";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {useUserControls} from "@/components/features/gameWindow/hooks";
import {UserGroup} from "./UserGroup";
import {OpenLetter} from "@/components/ui/icons";

interface UserManagerPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const UserManagerPanel = ({isOpen, onClose}: UserManagerPanelProps) => {
    const {discordTown} = useDiscordTown();
    const {
        inviteAll,
        isLoading: isControlsLoading,
        canRun: userControlsCanRun
    } = useUserControls();

    return (
        <BasePanel title="User Manager" isOpen={isOpen} onClose={onClose}>
            <div className="user-manager">
                <div className="column user-list">
                    <UserGroup className="user-group-storytellers" title="StoryTellers" users={discordTown?.storyTellers}/>
                    <UserGroup className="user-group-players" title="Players" users={discordTown?.players}/>
                    <UserGroup className="user-group-spectators" title="Spectators" users={discordTown?.spectators}/>

                    <hr className="my-4 border-white"/>
                    {isControlsLoading && <Spinner/>}
                    {userControlsCanRun &&
                        <IconButton
                            icon={<OpenLetter/>}
                            text="Invite all Players"
                            variant="primary"
                            onClick={inviteAll}
                        />
                    }
                </div>

                <div className="column add-users-view">
                    <h3 className="title">Available Users</h3>
                </div>
            </div>
        </BasePanel>
    )
};



