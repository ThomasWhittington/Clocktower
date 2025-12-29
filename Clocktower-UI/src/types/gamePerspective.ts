import type {ClocktowerServerDataGamePerspective} from "@/api";
import {type GameUser, mapToGameUser} from "@/types/gameUser.ts";
import {UserType} from "@/types/userType.ts";

export type GamePerspective = {
    id: string;
    userId: string;
    guildId: string,
    users: GameUser[];
    players: GameUser[];
    spectators: GameUser[];
    storyTellers: GameUser[];
    createdDate: Date,
    createdBy: GameUser
}

export function mapToGamePerspective(apiPerspective: ClocktowerServerDataGamePerspective): GamePerspective {
    if (!apiPerspective.id || !apiPerspective.userId || !apiPerspective.guildId) {
        throw new Error('Invalid API response: missing required fields');
    }
    return {
        id: apiPerspective.id,
        userId: apiPerspective.userId,
        guildId: apiPerspective.guildId,
        users: (apiPerspective.users ?? []).map(mapToGameUser),
        players: (apiPerspective.players ?? []).map(mapToGameUser),
        spectators: (apiPerspective.spectators ?? []).map(mapToGameUser),
        storyTellers: (apiPerspective.storyTellers ?? []).map(mapToGameUser),
        createdDate: apiPerspective.createdDate ?? new Date(),
        createdBy: apiPerspective.createdBy ? mapToGameUser(apiPerspective.createdBy) : {id: '', isPlaying: false, userType: UserType.Unknown}
    };
}