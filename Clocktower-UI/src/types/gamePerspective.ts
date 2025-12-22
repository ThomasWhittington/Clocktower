import type {ClocktowerServerDataGamePerspective} from "@/api";
import {type GameUser, mapToGameUser} from "@/types/gameUser.ts";

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
    console.log(apiPerspective);
    return {
        id: apiPerspective.id!,
        userId: apiPerspective.userId!,
        guildId: apiPerspective.guildId!,
        users: apiPerspective.users!.map(mapToGameUser),
        players: apiPerspective.players!.map(mapToGameUser),
        spectators: apiPerspective.spectators!.map(mapToGameUser),
        storyTellers: apiPerspective.storyTellers!.map(mapToGameUser),
        createdDate: apiPerspective.createdDate!,
        createdBy: mapToGameUser(apiPerspective.createdBy!)
    };
}