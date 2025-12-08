import type {
    ClocktowerServerDataGameUser
} from "@/api";
import {
    mapToVoiceState,
    type VoiceState
} from "@/types/voiceState.ts";

export type GameUser = {
    id: string;
    name: string;
    avatarUrl: string | null;
    isPlaying: boolean,
    isPresent: boolean,
    voiceState: VoiceState | null
};

export function mapToGameUser(apiGameUser: ClocktowerServerDataGameUser): GameUser {
    return {
        id: apiGameUser.id!,
        name: apiGameUser.name!,
        avatarUrl: apiGameUser.avatarUrl ?? null,
        isPlaying: apiGameUser.isPlaying ?? false,
        isPresent: apiGameUser.isPresent ?? false,
        voiceState: apiGameUser.voiceState ? mapToVoiceState(apiGameUser.voiceState) : null
    };
}