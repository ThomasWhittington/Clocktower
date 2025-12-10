import type {
    ClocktowerServerDataGameUser
} from "@/api";
import {
    UserType
} from "@/types";

export type GameUser = {
    id: string;
    isPlaying: boolean;
    userType: UserType
};

export function mapToGameUser(apiGameUser: ClocktowerServerDataGameUser): GameUser {
    return {
        id: apiGameUser.id!,
        isPlaying: apiGameUser.isPlaying ?? false,
        userType: (UserType[apiGameUser.userType as keyof typeof UserType]) ?? UserType.Unknown
    };
}