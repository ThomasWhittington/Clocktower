import {
    mapToPlayer,
    type Player
} from "./player.ts";
import type {
    ClocktowerServerDataGameState
} from "@/api";

export type GameState = {
    id: string;
    guildId: string,
    maxPlayers: number;
    players: Player[];
    isFull: boolean;
    createdDate: Date,
    createdBy: string
}

export function mapToGameState(apiGame: ClocktowerServerDataGameState): GameState {
    const players = (apiGame.players ?? [])
        .map(player => mapToPlayer(player));

console.log(apiGame)
    return {
        id: apiGame.id ?? '',
        guildId: apiGame.guildId ?? '',
        maxPlayers: apiGame.maxPlayers ?? 0,
        players: players,
        isFull: apiGame.isFull ?? false,
        createdDate:apiGame.createdDate ?? new Date(2000, 1, 1),
        createdBy: apiGame.createdBy ?? ''
    };
}