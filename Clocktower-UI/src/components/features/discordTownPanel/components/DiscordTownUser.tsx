import type {
    MiniUser
} from "@/types";

function DiscordTownUser({user}: {
    user: MiniUser
}) {
    return (
        <div
            className="town-user-status">
            <img
                src={user.avatarUrl}
                alt={user.name}/>
            <p>{user.name}</p>
        </div>
    );
}

export default DiscordTownUser;