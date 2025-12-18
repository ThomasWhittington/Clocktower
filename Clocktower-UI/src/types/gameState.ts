import type {ClocktowerServerDataGameState} from "@/api";
import {type GameUser, mapToGameUser} from "@/types/gameUser.ts";

export type GameState = {
    id: string;
    guildId: string,
    players: GameUser[];
    spectators: GameUser[];
    storyTellers: GameUser[];
    createdDate: Date,
    createdBy: GameUser
}

export function mapToGameState(apiGame: ClocktowerServerDataGameState): GameState {
    return {
        id: apiGame.id!,
        guildId: apiGame.guildId!,
        players: apiGame.players!.map(mapToGameUser),
        spectators: apiGame.spectators!.map(mapToGameUser),
        storyTellers: apiGame.storyTellers!.map(mapToGameUser),
        createdDate: apiGame.createdDate!,
        createdBy: mapToGameUser(apiGame.createdBy!)
    };
}