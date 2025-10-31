import type {
    components
} from "../openApi/clocktowerServer";
import {
    mapToPlayer,
    type Player
} from "./player.ts";

export type GameState = {
    id: string;
    maxPlayers: number;
    players: Player[];
    isFull: boolean;
}


type OpenApiGame = components["schemas"]["Clocktower.Server.Data.GameState"];

export function mapToGameState(apiGame: OpenApiGame): GameState {
    const players = (apiGame.players ?? [])
        .map(player => mapToPlayer(player));


    return {
        id: apiGame.id ?? '',
        maxPlayers: apiGame.maxPlayers ?? 0,
        players: players,
        isFull: apiGame.isFull ?? false
    };
}