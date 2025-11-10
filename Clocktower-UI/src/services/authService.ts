import {
    type User
} from '@/types/auth';

export const authService = {
    getUser(): User | null {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    },

    clearUser(): void {
        localStorage.removeItem('user');
    },

    initiateDiscordLogin(): void {
        window.location.href = import.meta.env.VITE_CLOCKTOWER_SERVER_URI + '/api/discord/auth';
    }
};