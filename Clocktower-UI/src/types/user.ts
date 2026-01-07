import type {ClocktowerServerDataDtoUserDto} from "@/api";
import {mapToVoiceState, type VoiceState} from "@/types/voiceState.ts";
import {mapToRole, type Role, UserType} from "@/types";

export type User = {
    id: string;
    name: string;
    avatarUrl: string | undefined;
    isPresent: boolean,
    voiceState: VoiceState | undefined;
    isPlaying: boolean;
    userType: UserType;
    seatingPosition: number;
    hasVoteToken: boolean;
    isDead: boolean;
    isMarked: boolean;
    role: Role | undefined;
};

export function mapToUser(userDto: ClocktowerServerDataDtoUserDto): User {
    return {
        id: userDto.id!,
        name: userDto.name!,
        avatarUrl: userDto.avatarUrl ?? undefined,
        isPresent: userDto.isPresent ?? false,
        voiceState: userDto.voiceState ? mapToVoiceState(userDto.voiceState) : undefined,
        isPlaying: userDto.isPlaying ?? false,
        userType: (UserType[userDto.userType as keyof typeof UserType]) ?? UserType.Unknown,
        seatingPosition: userDto.seatingPosition ?? -1,
        hasVoteToken: userDto.hasVoteToken ?? false,
        isDead: userDto.isDead ?? false,
        isMarked: userDto.isMarked ?? false,
        role: userDto.role ? mapToRole(userDto.role) : undefined
    };
}