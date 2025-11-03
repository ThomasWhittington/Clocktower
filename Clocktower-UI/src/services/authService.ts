import {
    type User
} from '../types/auth';

export const authService = {
    async getUserData(key: string): Promise<User> {
        const response = await fetch(`http://localhost:5120/api/discord/auth/data/${key}`); //TODO use openapi
        if (!response.ok) {
            throw new Error('Failed to retrieve auth data');
        }
        return response.json();
    },

    saveUser(user: User): void {
        localStorage.setItem('user', JSON.stringify(user));
    },

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