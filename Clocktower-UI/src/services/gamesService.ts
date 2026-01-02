import {type GamePerspective, mapToGamePerspective, mapToUser, type User,} from "@/types";
import {addUserToGameApi, getAvailableGameUsersApi, getGamesApi, loadDummyGamesApi, removeUserFromGameApi, startGameApi} from "@/api";
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

async function getAvailableGameUsers(gameId: string): Promise<User[]> {
    const {
        data,
        error
    } = await getAvailableGameUsersApi({
        client: apiClient,
        path: {
            gameId: gameId,
        }
    });

    if (error) {
        console.error('Failed to get available users:', error);
        throw new Error(error.toString());
    }

    return data?.map(mapToUser) ?? [];
}

async function addUserToGame(gameId: string, userId: string): Promise<string> {
    const {
        data,
        error
    } = await addUserToGameApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to add user:', error);
        throw new Error(error.toString());
    }
    return data;
}

async function removeUserFromGame(gameId: string, userId: string): Promise<string> {
    const {
        data,
        error
    } = await removeUserFromGameApi({
        client: apiClient,
        path: {
            gameId: gameId,
            userId: userId
        }
    });

    if (error) {
        console.error('Failed to remove user:', error);
        throw new Error(error.toString());
    }

    return data;
}

export const gamesService = {
    getGames,
    loadDummyData,
    startGame,
    getAvailableGameUsers,
    addUserToGame,
    removeUserFromGame
}