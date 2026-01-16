import type {ClocktowerServerDataDtoUserDto} from "@/api";
import {mapToVoiceState, type VoiceState} from "@/types/voiceState.ts";
import {mapToRole, Role, UserType} from "@/types";

export class User {
    readonly id: string;
    readonly name: string;
    readonly avatarUrl: string | undefined;
    readonly isPresent: boolean;
    readonly voiceState: VoiceState | undefined;
    readonly isPlaying: boolean;
    readonly userType: UserType;
    readonly seatingPosition: number;
    readonly hasVoteToken: boolean;
    readonly isDead: boolean;
    readonly isMarked: boolean;
    readonly role: Role | undefined;
    readonly draftRole: Role | undefined;

    constructor(data: Partial<User>) {
        this.id = data.id ?? '';
        this.name = data.name ?? '';
        this.avatarUrl = data.avatarUrl;
        this.isPresent = data.isPresent ?? false;
        this.voiceState = data.voiceState;
        this.isPlaying = data.isPlaying ?? false;
        this.userType = data.userType ?? UserType.Unknown;
        this.seatingPosition = data.seatingPosition ?? -1;
        this.hasVoteToken = data.hasVoteToken ?? false;
        this.isDead = data.isDead ?? false;
        this.isMarked = data.isMarked ?? false;
        this.role = data.role instanceof Role ? data.role : (data.role ? mapToRole(data.role as any) : undefined);
        this.draftRole = data.draftRole instanceof Role ? data.draftRole : (data.draftRole ? mapToRole(data.draftRole as any) : undefined);
    }
}

export function mapToUser(userDto: ClocktowerServerDataDtoUserDto): User {
    return new User({
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
        role: userDto.role ? mapToRole(userDto.role) : undefined,
        draftRole: userDto.draftRole ? mapToRole(userDto.draftRole) : undefined
    });
}