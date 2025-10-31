import {
    createPathBasedClient
} from "openapi-fetch";
import type {
    paths
} from "../openApi/clocktowerServer";
import {
    type GameState,
    mapToGameState
} from "../types/game.ts";

const client = createPathBasedClient<paths>();

async function getGame(id: string): Promise<GameState> {
    const {
        data,
        error
    } = await client["/api/games/{gameId}"].GET({
        params: {
            path: {"gameId": id}
        }
    });

    if (error) {
        console.error('Failed to get game:', error);
        throw new Error('Failed to get game');
    }

    if (data) {
        return mapToGameState(data);
    }
    return {
        id: id,
        players: [],
        maxPlayers: 0,
        isFull: false
    };
}

async function getGames(): Promise<GameState[]> {
    const {
        data,
        error
    } = await client["/api/games"].GET();

    if (error) {
        console.error('Failed to fetch games:', error);
        throw new Error('Failed to fetch games');
    }

    return data?.map(mapToGameState) ?? [];
}

async function loadDummyData(): Promise<string | undefined> {
    const {
        data,
        error
    } = await client["/api/games/load"].POST();

    if (error) {
        console.error('Failed to load dummy data:', error);
        throw new Error('Failed to load dummy data');
    }

    return data;
}

async function startGame(id: string): Promise<GameState> {
    const {
        data,
        error
    } = await client["/api/games/{gameId}/start"].POST({
        params: {
            path: {"gameId": id}
        }
    });

    if (error) {
        console.error('Failed to start game:', error);
        throw new Error('Failed to start game');
    }

    if (data) {
        return mapToGameState(data);
    }
    return {
        id: id,
        players: [],
        maxPlayers: 0,
        isFull: false
    };
}

export const gamesService = {
    getGame,
    getGames,
    loadDummyData,
    startGame
}