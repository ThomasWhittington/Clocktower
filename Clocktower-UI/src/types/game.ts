import {
    mapToPlayer,
    type Player
} from "./player.ts";
import type {
    ClocktowerServerDataGameState
} from "@/generated";

export type GameState = {
    id: string;
    maxPlayers: number;
    players: Player[];
    isFull: boolean;
}

export function mapToGameState(apiGame: ClocktowerServerDataGameState): GameState {
    const players = (apiGame.players ?? [])
        .map(player => mapToPlayer(player));


    return {
        id: apiGame.id ?? '',
        maxPlayers: apiGame.maxPlayers ?? 0,
        players: players,
        isFull: apiGame.isFull ?? false
    };
}