import {
    type User,
    UserType,
} from "@/types";
import {
    DiscordUserVoiceStatus,
    UserAvatar
} from "@/components/ui";

function DiscordTownUser({user}: Readonly<{
    user: User
}>) {

    const typeColorMap: Record<UserType, string> = {
        [UserType.Player]: 'text-player',
        [UserType.Spectator]: 'text-spectator',
        [UserType.StoryTeller]: 'text-storyteller',
        [UserType.Unknown]: 'text-unknown'
    };
    const colorClass = typeColorMap[user.userType ?? UserType.Unknown];
    const iconSize = 20;

    return (
        <div className="town-user-status">
            {user.avatarUrl &&
                <UserAvatar user={user} size={32} className="discord-user-avatar shrink-0"/>
            }
            <p className={`${colorClass} truncate min-w-0`}>{user.name}</p>
            {user.voiceState && (
                <DiscordUserVoiceStatus voiceState={user.voiceState} iconSize={iconSize}/>
            )}
        </div>
    );
}

export default DiscordTownUser;