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

export const gamesService = {
    getGame,
    getGames,
    loadDummyData
}