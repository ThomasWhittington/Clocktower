import {
    type User,
    UserType
} from "@/types";
import {
    IconButton,
    Spinner
} from "@/components/ui";
import {UserAvatar} from "@/components/ui/UserAvatar.tsx";
import {UserTypeChange} from "./UserTypeChange";
import {
    ArrowRightCircle,
    OpenLetter,
    RemoveIcon
} from "@/components/ui/icons";
import {UserUtils} from "@/utils";
import {TokenRoleIcon} from "@/components/tokens";

interface UserRowProps {
    user: User,
    controlsLoading: boolean,
    controlsCanRun: boolean,
    inviteUser: (user: User) => Promise<void>,
    removeUser: (user: User) => Promise<void>,
    changeUserType: (user: User, type: UserType) => void
}

export const UserRow = ({user, controlsLoading, controlsCanRun, removeUser, inviteUser, changeUserType}: UserRowProps) => {
    const isStoryteller = UserUtils.isStoryTeller(user);
    return (
        <div className={`user-row user-row-${user.id}`}>
            <div className="user-row-section">
                {controlsLoading && <Spinner/>}
                {controlsCanRun &&
                    <IconButton
                        icon={[<OpenLetter key="openLetter"/>, <ArrowRightCircle key="arrowRightCircle"/>]}
                        variant="primary"
                        onClick={() => inviteUser(user)}
                        tooltip="Invite user to the game"
                    />
                }
                <UserAvatar user={user} size={48} className="discord-user-avatar"/>
                <p>{user.name}</p>
            </div>
            <div className="user-row-section">
                {user?.role && <TokenRoleIcon roleId={user.role.id} className="role-icon"/>}
                <UserTypeChange user={user} isLoading={controlsLoading} canRun={controlsCanRun} changeUserType={changeUserType}/>
                <IconButton
                    className={isStoryteller ? "invisible" : "visible"}
                    icon={<RemoveIcon/>}
                    variant="danger"
                    onClick={() => removeUser(user)}
                    tooltip="Remove user from the game"
                />
            </div>
        </div>
    );
}