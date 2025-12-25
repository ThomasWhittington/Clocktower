import type {ClocktowerServerDataDtoUserDto} from "@/api";
import {mapToVoiceState, type VoiceState} from "@/types/voiceState.ts";
import {UserType} from "@/types/userType.ts";

export type User = {
    id: string;
    name: string;
    avatarUrl: string | undefined;
    isPresent: boolean,
    voiceState: VoiceState | undefined;
    isPlaying: boolean;
    userType: UserType
};

export function mapToUser(userDto: ClocktowerServerDataDtoUserDto): User {
    return {
        id: userDto.id!,
        name: userDto.name!,
        avatarUrl: userDto.avatarUrl ?? undefined,
        isPresent: userDto.isPresent ?? false,
        voiceState: userDto.voiceState ? mapToVoiceState(userDto.voiceState) : undefined,
        isPlaying: userDto.isPlaying ?? false,
        userType: (UserType[userDto.userType as keyof typeof UserType]) ?? UserType.Unknown
    };
}