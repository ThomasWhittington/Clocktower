import type {
    GameUser
} from "@/types";

function DiscordTownUser({user}: Readonly<{
    user: GameUser
}>) {
    return (
        <div
            className="town-user-status">
            {user.avatarUrl &&
                <img
                    src={user.avatarUrl}
                    alt={user.name}/>
            }
            <p>{user.name}</p>
        </div>
    );
}

export default DiscordTownUser;