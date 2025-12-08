import type {
    ClocktowerServerDataVoiceState
} from "@/api";

export type VoiceState = {
    isServerMuted: boolean,
    isServerDeafened: boolean,
    isSelfMuted: boolean,
    isSelfDeafened: boolean
}

export function mapToVoiceState(apiVoiceState: ClocktowerServerDataVoiceState): VoiceState {
    return {
        isServerMuted: apiVoiceState.isServerMuted ?? false,
        isServerDeafened: apiVoiceState.isServerDeafened ?? false,
        isSelfMuted: apiVoiceState.isSelfMuted ?? false,
        isSelfDeafened: apiVoiceState.isSelfDeafened ?? false
    };
}