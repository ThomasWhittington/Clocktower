import type {
    MiniUser
} from "../../../../types";

function DiscordTownUser({user}: {
    user: MiniUser
}) {
    return (
        <>
            <p className="text-purple-700">{user.name} {user.id}</p>
        </>
    );
}

export default DiscordTownUser;