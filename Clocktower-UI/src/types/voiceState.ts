import type {ClocktowerServerDataVoiceState} from "@/api";

export type VoiceState = {
    isServerMuted: boolean,
    isServerDeafened: boolean,
    isSelfMuted: boolean,
    isSelfDeafened: boolean,
    isPresent: boolean
}

export function mapToVoiceState(apiVoiceState: ClocktowerServerDataVoiceState): VoiceState {
    return {
        isPresent: apiVoiceState.isPresent ?? false,
        isServerMuted: apiVoiceState.isServerMuted ?? false,
        isServerDeafened: apiVoiceState.isServerDeafened ?? false,
        isSelfMuted: apiVoiceState.isSelfMuted ?? false,
        isSelfDeafened: apiVoiceState.isSelfDeafened ?? false
    };
}