import type {VoiceState} from "@/types/voiceState.ts";
import {SelfDeafenedIcon, SelfMutedIcon, ServerDeafenedIcon, ServerMutedIcon} from "@/components/ui/icons";

export const DiscordUserVoiceStatus = ({voiceState, iconSize = 20}: {
    voiceState: VoiceState,
    iconSize?: number
}) => {
    return (
        <div className="discord-user-voice-status">
            {voiceState.isSelfMuted && !voiceState.isServerMuted &&
                <SelfMutedIcon width={iconSize} height={iconSize}/>
            }
            {voiceState.isServerMuted &&
                <ServerMutedIcon width={iconSize} height={iconSize}/>
            }
            {voiceState.isSelfDeafened && !voiceState.isServerDeafened &&
                <SelfDeafenedIcon width={iconSize} height={iconSize}/>
            }
            {voiceState.isServerDeafened &&
                <ServerDeafenedIcon width={iconSize} height={iconSize}/>
            }
        </div>
    );
}