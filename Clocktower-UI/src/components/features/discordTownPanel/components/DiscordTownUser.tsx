import type {
    MiniUser
} from "../../../../types";

function DiscordTownUser({user}: {
    user: MiniUser
}) {
    return (
        <>
            <p className="text-purple-700">{user.name}</p>
        </>
    );
}

export default DiscordTownUser;