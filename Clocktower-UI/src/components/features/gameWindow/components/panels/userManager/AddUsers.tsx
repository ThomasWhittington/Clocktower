import {
    RoleDistributionWidget,
    UserAvatar
} from "@/components/ui";
import {type User} from "@/types";

interface AddUsersProps {
    availableUsers: User[];
    addUserToGame: (user: User) => Promise<void>;
}

export const AddUsers = ({availableUsers, addUserToGame}: AddUsersProps) => {
    return (
        <div className="column add-users-view">
            <RoleDistributionWidget/>
            <h3 className="title">Available Users</h3>
            <div className="users-container">
                {availableUsers.map(user =>
                    <button key={user.id} className="available-user" onClick={() => addUserToGame(user)}>
                        <UserAvatar user={user} size={48} className="discord-user-avatar"/>
                        <p>{user.name}</p>
                    </button>
                )}
            </div>
        </div>
    );
}