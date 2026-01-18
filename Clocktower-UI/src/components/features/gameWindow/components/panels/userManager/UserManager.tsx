import {
    OpenLetter,
    Randomise
} from "@/components/ui/icons";
import {
    IconButton,
    Spinner
} from "@/components/ui";
import {UserGroup} from "@/components/features/gameWindow/components/panels/userManager/UserGroup.tsx";
import {
    DiscordTown,
    type User,
    type UserType
} from "@/types";

interface UserManagerProps {
    inviteAll: () => Promise<void>;
    inviteUser: (user: User) => Promise<void>;
    changeUserType: (user: User, userType: UserType) => Promise<void>;
    removeUser: (user: User) => Promise<void>;
    randomiseSeatingPositions: () => Promise<void>;
    discordTown: DiscordTown | undefined,
    isLoading: boolean;
    canRun: boolean;
}

export const UserManager = ({
                                inviteAll,
                                randomiseSeatingPositions,
                                isLoading,
                                canRun,
                                inviteUser,
                                removeUser,
                                changeUserType,
                                discordTown
                            }: UserManagerProps) => {
    return (
        <div className="column user-list">

            {canRun &&
                <IconButton
                    icon={<Randomise/>}
                    className="ml-auto"
                    text="Randomise seat positions"
                    variant="secondary"
                    onClick={randomiseSeatingPositions}
                />
            }

            <UserGroup
                className="user-group-storytellers"
                title="StoryTellers"
                users={discordTown?.storyTellers}
                controlsLoading={isLoading}
                controlsCanRun={canRun}
                inviteUser={inviteUser}
                removeUser={removeUser}
                changeUserType={changeUserType}
            />
            <UserGroup
                className="user-group-players"
                title="Players"
                users={discordTown?.players}
                controlsLoading={isLoading}
                controlsCanRun={canRun}
                inviteUser={inviteUser}
                removeUser={removeUser}
                changeUserType={changeUserType}
            />
            <UserGroup
                className="user-group-spectators"
                title="Spectators"
                users={discordTown?.spectators}
                controlsLoading={isLoading}
                controlsCanRun={canRun}
                inviteUser={inviteUser}
                removeUser={removeUser}
                changeUserType={changeUserType}
            />

            <hr className="my-4 border-white"/>
            {isLoading && <Spinner/>}
            {canRun &&
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