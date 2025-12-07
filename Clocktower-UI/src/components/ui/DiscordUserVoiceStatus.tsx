import type {
    VoiceState
} from "@/types/voiceState.ts";

export const DiscordUserVoiceStatus = ({voiceState}: {
    voiceState: VoiceState
}) => {
    return (
        <>
            {voiceState.isSelfMuted &&
                <p>sm</p>}
            {voiceState.isSelfDeafened &&
                <p>sd</p>}
            {voiceState.isServerMuted &&
                <p>m</p>}
            {voiceState.isServerDeafened &&
                <p>d</p>}
        </>
    );
}