export const useAddBot = (): {
    addBot: () => void
} => {
    const addBot = () => {
        window.location.href = 'http://localhost:5120/api/discord-auth/addBot';
    };
    return {
        addBot
    }
};