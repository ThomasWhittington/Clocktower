import {UserRow} from "./UserRow";
import {
    type User,
    UserType
} from "@/types";

interface UserGroupProps {
    className?: string;
    title: string;
    users: User[] | undefined;
    controlsLoading: boolean,
    controlsCanRun: boolean,
    inviteUser: (user: User) => Promise<void>,
    removeUser: (user: User) => Promise<void>,
    changeUserType: (user: User, type: UserType) => void
}

export const UserGroup = ({className, title, users, controlsLoading, controlsCanRun, inviteUser, removeUser, changeUserType}: UserGroupProps) => (
    users && users.length > 0 &&
    <div className={className}>
        <h3 className="title">{title}</h3>
        {users.map((user) => {
            return <UserRow
                user={user}
                key={user.id}
                controlsLoading={controlsLoading}
                controlsCanRun={controlsCanRun}
                inviteUser={inviteUser}
                removeUser={removeUser}
                changeUserType={changeUserType}
            />
        })}
    </div>
);