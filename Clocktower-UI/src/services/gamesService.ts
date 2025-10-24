import axios
    from "axios";

const endpoint = '/api/games';

async function getGame(id: string) {
    const result = await axios.get(`${endpoint}/${id}`);
    return result.data;
}

async function getGames() {
    const result = await axios.get(endpoint);
    return result.data || [];
}

async function loadDummyData() {
    const result = await axios.post(`${endpoint}/load`);
    return result.data;
}

async function startGame(id: string) {
    const result = await axios.post(`${endpoint}/${id}/start`);
    if (result.status === 201) {
        return result.data;
    } else {
        console.error(result)
    }

    return result.data;
}

export const gamesService = {
    getGame,
    getGames,
    loadDummyData,
    startGame
}