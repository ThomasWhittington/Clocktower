import type {VoiceState} from "@/types/voiceState.ts";
import {SelfDeafenedIcon, SelfMutedIcon, ServerDeafenedIcon, ServerMutedIcon} from "@/components/ui/icons";

export const DiscordUserVoiceStatus = ({voiceState}: {
    voiceState: VoiceState
}) => {
    const iconWidth = 20;
    return (
        <div className="discord-user-voice-status">
            {voiceState.isSelfMuted && !voiceState.isServerMuted &&
                <SelfMutedIcon width={iconWidth}/>
            }
            {voiceState.isServerMuted &&
                <ServerMutedIcon width={iconWidth}/>
            }
            {voiceState.isSelfDeafened && !voiceState.isServerDeafened &&
                <SelfDeafenedIcon width={iconWidth}/>
            }
            {voiceState.isServerDeafened &&
                <ServerDeafenedIcon width={iconWidth}/>
            }
        </div>
    );
}