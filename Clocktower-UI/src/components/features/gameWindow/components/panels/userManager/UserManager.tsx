import {useUserControls} from "@/components/features/gameWindow/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {OpenLetter} from "@/components/ui/icons";
import {IconButton, Spinner} from "@/components/ui";
import {UserGroup} from "@/components/features/gameWindow/components/panels/userManager/UserGroup.tsx";

export const UserManager = () => {
    const {discordTown} = useDiscordTown();
    const {
        inviteAll,
        isLoading: isControlsLoading,
        canRun: userControlsCanRun
    } = useUserControls();

    return (
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
    );
}
