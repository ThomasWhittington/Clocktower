import type {
    TownUser
} from "@/types";
import {
    DiscordUserVoiceStatus
} from "@/components/ui";

function DiscordTownUser({user}: Readonly<{
    user: TownUser
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
            {
                user.voiceState &&
                <DiscordUserVoiceStatus voiceState={user.voiceState}/>
            }
        </div>
    );
}

export default DiscordTownUser;