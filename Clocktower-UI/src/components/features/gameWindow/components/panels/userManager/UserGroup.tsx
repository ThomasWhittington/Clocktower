import {UserRow} from "./UserRow";
import type {User} from "@/types";

export const UserGroup = ({className, title, users}: { className?: string, title: string, users: User[] | undefined }) => (
    users && users.length > 0 &&
    <div className={className}>
        <h3 className="title">{title}</h3>
        {users.map((user) => {
            return <UserRow user={user} key={user.id}/>
        })}
    </div>
);