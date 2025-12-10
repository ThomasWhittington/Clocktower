import type {
    ClocktowerServerDataTownUser
} from "@/api";
import {
    mapToVoiceState,
    type VoiceState
} from "@/types/voiceState.ts";

export type TownUser = {
    id: string;
    name: string;
    avatarUrl: string | null;
    isPresent: boolean,
    voiceState: VoiceState | null
};

export function mapToTownUser(apiGameUser: ClocktowerServerDataTownUser): TownUser {
    return {
        id: apiGameUser.id!,
        name: apiGameUser.name!,
        avatarUrl: apiGameUser.avatarUrl ?? null,
        isPresent: apiGameUser.isPresent ?? false,
        voiceState: apiGameUser.voiceState ? mapToVoiceState(apiGameUser.voiceState) : null
    };
}