import type {
    ClocktowerServerDataUserDto
} from "@/api";
import {
    mapToVoiceState,
    type VoiceState
} from "@/types/voiceState.ts";
import {
    UserType
} from "@/types/userType.ts";

export type User = {
    id: string;
    name: string;
    avatarUrl: string | null;
    isPresent: boolean,
    voiceState: VoiceState | null;
    isPlaying: boolean;
    userType: UserType
};

export function mapToUser(userDto: ClocktowerServerDataUserDto): User {
    return {
        id: userDto.id!,
        name: userDto.name!,
        avatarUrl: userDto.avatarUrl ?? null,
        isPresent: userDto.isPresent ?? false,
        voiceState: userDto.voiceState ? mapToVoiceState(userDto.voiceState) : null,
        isPlaying: userDto.isPlaying ?? false,
        userType: (UserType[userDto.userType as keyof typeof UserType]) ?? UserType.Unknown
    };
}