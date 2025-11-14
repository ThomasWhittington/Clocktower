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
    isFull: boolean;
    createdDate: Date,
    createdBy: GameUser
}

export function mapToGameState(apiGame: ClocktowerServerDataGameState): GameState {
    let players: GameUser[] = [];
    if (apiGame.players!.length > 0) {
        players = apiGame.players!
            .map(player => mapToGameUser(player));
    }

    return {
        id: apiGame.id!,
        guildId: apiGame.guildId!,
        maxPlayers: apiGame.maxPlayers!,
        players: players,
        isFull: apiGame.isFull!,
        createdDate: apiGame.createdDate!,
        createdBy: mapToGameUser(apiGame.createdBy!)
    };
}