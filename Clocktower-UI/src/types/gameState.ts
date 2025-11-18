import type {
    ClocktowerServerDataGameState
} from "@/api";
import {
    type GameUser,
    mapToGameUser
} from "@/types/gameUser.ts";

export type GameState = {
    id: string;
    guildId: string,
    maxPlayers: number;
    players: GameUser[];
    spectators: GameUser[];
    storyTellers: GameUser[];
    isFull: boolean;
    createdDate: Date,
    createdBy: GameUser
}

export function mapToGameState(apiGame: ClocktowerServerDataGameState): GameState {
    return {
        id: apiGame.id!,
        guildId: apiGame.guildId!,
        maxPlayers: apiGame.maxPlayers!,
        players: apiGame.players!.map(mapToGameUser),
        spectators: apiGame.spectators!.map(mapToGameUser),
        storyTellers: apiGame.storyTellers!.map(mapToGameUser),
        isFull: apiGame.isFull!,
        createdDate: apiGame.createdDate!,
        createdBy: mapToGameUser(apiGame.createdBy!)
    };
}