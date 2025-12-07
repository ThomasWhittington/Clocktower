import type {
    VoiceState
} from "@/types/voiceState.ts";
import SelfMutedIcon
    from "@/../public/icons/selfMuted.svg?react";
import ServerMutedIcon
    from "@/../public/icons/serverMuted.svg?react";
import SelfDeafenedIcon
    from "@/../public/icons/selfDeafened.svg?react";
import ServerDeafenedIcon
    from "@/../public/icons/serverDeafened.svg?react";

export const DiscordUserVoiceStatus = ({voiceState}: {
    voiceState: VoiceState
}) => {
    const iconWidth = 20;
    return (
        <div className="discord-user-voice-status">
            {voiceState.isSelfMuted && !voiceState.isServerMuted &&
                <SelfMutedIcon
                    width={iconWidth}/>
            }
            {voiceState.isServerMuted &&
                <ServerMutedIcon
                    width={iconWidth}/>
            }
            {voiceState.isSelfDeafened && !voiceState.isServerDeafened &&
                <SelfDeafenedIcon
                    width={iconWidth}/>
            }
            {voiceState.isServerDeafened &&
                <ServerDeafenedIcon
                    width={iconWidth}/>
            }
        </div>
    );
}