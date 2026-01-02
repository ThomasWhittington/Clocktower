import type {User} from "@/types";
import {useUserControls} from "@/components/features/gameWindow/hooks";
import {IconButton, Spinner} from "@/components/ui";
import {UserAvatar} from "@/components/ui/UserAvatar.tsx";
import {UserTypeChange} from "./UserTypeChange";
import {ArrowRightCircle, OpenLetter, RemoveIcon} from "@/components/ui/icons";

export const UserRow = ({user}: { user: User }) => {
    const {isLoading, canRun, inviteUser, removeUser} = useUserControls();
    return (
        <div className={`user-row user-row-${user.id}`}>
            <div className="user-row-section">
                {isLoading && <Spinner/>}
                {canRun &&
                    <IconButton
                        icon={[<OpenLetter key="openLetter"/>, <ArrowRightCircle key="arrowRightCircle"/>]}
                        variant="primary"
                        onClick={() => inviteUser(user)}
                    />
                }
                <UserAvatar user={user} size={48} className="discord-user-avatar"/>
                <p>{user.name}</p>
            </div>
            <div className="user-row-section">
                <UserTypeChange user={user}/>
                <IconButton icon={<RemoveIcon/>} variant="danger" onClick={() => removeUser(user)}/>
            </div>
        </div>
    );
}