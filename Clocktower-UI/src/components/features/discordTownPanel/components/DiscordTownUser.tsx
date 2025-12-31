import {type User, UserType} from "@/types";
import {DiscordUserVoiceStatus, UserAvatar} from "@/components/ui";

function DiscordTownUser({user}: Readonly<{
    user: User
}>) {
    const userType = user.userType ? UserType[user.userType] : UserType.Unknown;
    return (
        <div
            className="town-user-status">
            {user.avatarUrl &&
                <UserAvatar user={user} size={32} className="discord-user-avatar"/>
            }
            <p>({userType}) {user.name}</p>
            {
                user.voiceState &&
                <DiscordUserVoiceStatus voiceState={user.voiceState}/>
            }
        </div>
    );
}

export default DiscordTownUser;