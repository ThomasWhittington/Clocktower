export const useAddBot = (): {
    addBot: () => void
} => {
    const addBot = () => {
        window.location.href = import.meta.env.VITE_CLOCKTOWER_SERVER_URI + '/api/discord/auth/addBot';
    };
    return {
        addBot
    }
};