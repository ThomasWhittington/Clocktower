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
        window.location.href = 'http://localhost:5120/api/discord/auth';
    }
};