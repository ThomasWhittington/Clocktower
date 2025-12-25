import {type GamePerspective, mapToGamePerspective,} from "@/types";
import {getGamesApi, loadDummyGamesApi, startGameApi} from "@/api";
import {apiClient} from "@/api/api-client.ts";

async function getGames(): Promise<GamePerspective[]> {

    const {
        data,
        error
    } = await getGamesApi({client: apiClient});

    if (error) {
        console.error('Failed to fetch games:', error);
        throw new Error('Failed to fetch games');
    }
    return data?.map(mapToGamePerspective) ?? [];
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

async function startGame(gameId: string, guildId: string, userId: string): Promise<GamePerspective | null> {
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
    return mapToGamePerspective(data);
}

export const gamesService = {
    getGames,
    loadDummyData,
    startGame
}