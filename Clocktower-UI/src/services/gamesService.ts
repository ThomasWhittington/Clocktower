import axios, {
    HttpStatusCode
} from "axios";

const endpoint = '/api/games';

async function getGame(id: string) {
    try {
        const result = await axios.get(`${endpoint}/${id}`);
        return result.data;
    } catch (error) {
        console.error('Failed to get game:', error);
        throw error;
    }
}

async function getGames() {
    try {
        const result = await axios.get(endpoint);
        return result.data || [];
    } catch (error) {
        console.error('Failed to get games:', error);
        return [];
    }
}

async function loadDummyData() {
    try {
        const result = await axios.post(`${endpoint}/load`);
        return result.data;
    } catch (error) {
        console.error('Failed to load dummy data:', error);
        throw error;
    }
}

async function startGame(id: string) {
    try {
        const result = await axios.post(`${endpoint}/${id}/start`);
        if (result.status === HttpStatusCode.Created) {
            return result.data;
        } else {
            console.error('Unexpected status:', result.status);
            return result.data;
        }
    } catch (error) {
        console.error('Failed to start game:', error);
        throw error;
    }
}

export const gamesService = {
    getGame,
    getGames,
    loadDummyData,
    startGame
}