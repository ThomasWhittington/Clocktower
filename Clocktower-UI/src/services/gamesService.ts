import {
    type GameState,
    mapToGameState,
} from "@/types";
import {
    getGameApi,
    getGamesApi,
    loadDummyGamesApi,
    startGameApi
} from "@/api";
import {
    apiClient
} from "@/api/api-client.ts";

async function getGame(id: string): Promise<GameState | null> {

    const {
        data,
        error
    } = await getGameApi({
        client: apiClient,
        path: {
            gameId: id
        }
    });

    if (error) {
        console.error('Failed to get game:', error);
        throw new Error('Failed to get game');
    }

    if (!data) return null;
    return mapToGameState(data);
}

async function getGames(): Promise<GameState[]> {

    const {
        data,
        error
    } = await getGamesApi({client: apiClient});

    if (error) {
        console.error('Failed to fetch games:', error);
        throw new Error('Failed to fetch games');
    }
    return data?.map(mapToGameState) ?? [];
}

async function getGamesInGuild(guildId: string): Promise<GameState[]> {

    const {
        data,
        error
    } = await getGamesApi({
        client: apiClient,
        query: {
            guildId: guildId
        }
    });

    if (error) {
        console.error('Failed to fetch games for guild:', error);
        throw new Error('Failed to fetch games for guild');
    }

    return data?.map(mapToGameState) ?? [];
}

async function loadDummyData(): Promise<string | undefined> {
    const {
        data,
        error
    } = await loadDummyGamesApi({client: apiClient});

    if (error) {
        console.error('Failed to load dummy data:', error);
        throw new Error('Failed to load dummy data');
    }

    return data;
}

async function startGame(gameId: string, guildId: string, userId: string): Promise<GameState | null> {
    const {
        data,
        error
    } = await startGameApi({
        client: apiClient,
        path: {
            guildId: guildId,
            gameId: gameId,
            userId: userId
        }
    });
    if (error) {
        console.error('Failed to start game:', error);
        throw new Error('Failed to start game');
    }

    if (!data) return null;
    return mapToGameState(data);
}

export const gamesService = {
    getGame,
    getGames,
    getGamesInGuild,
    loadDummyData,
    startGame
}