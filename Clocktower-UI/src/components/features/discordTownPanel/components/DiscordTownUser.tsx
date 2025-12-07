import type {
    GameUser
} from "@/types";
import {
    useUserVoiceStatus
} from "@/components/features/discordTownPanel/hooks";
import {
    DiscordUserVoiceStatus
} from "@/components/ui";

function DiscordTownUser({user}: Readonly<{
    user: GameUser
}>) {
    const {voiceStates} = useUserVoiceStatus(user.id);

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
                voiceStates &&
                <DiscordUserVoiceStatus voiceState={voiceStates}/>
            }
        </div>
    );
}

export default DiscordTownUser;