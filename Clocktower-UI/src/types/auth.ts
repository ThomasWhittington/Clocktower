export interface User {
    id: string;
    name: string;
    avatarUrl?: string;
}

export interface AuthState {
    user: User | null;
    isAuthenticated: boolean;
    isLoading: boolean;
}