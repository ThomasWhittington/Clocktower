import {
    create
} from 'zustand';
import type {
    User
} from "./types/auth.ts";

interface AppState {
    loggedIn: boolean,
    guildId: string,
    gameId: string,
    currentUser?: User,
    setGuildId: (value: string) => void;
    setGameId: (value: string) => void;
    setCurrentUser: (value: User) => void;
    clearSession: () => void;
    reset: () => void;
}

const getLoggedIn = (): boolean => {
    const currentUser = getStoredUser();
    return currentUser != undefined;
}

const getStoredGuildId = (): string => {
    return localStorage.getItem('guildId') || '';
};

const getStoredGameId = (): string => {
    return localStorage.getItem('gameId') || '';
};

const setStoredGuildId = (id: string) => {
    localStorage.setItem('guildId', id);
};

const setStoredGameId = (id: string) => {
    localStorage.setItem('gameId', id);
};

const getStoredUser = (): User | undefined => {
    const stored = localStorage.getItem('currentUser');
    return stored ? JSON.parse(stored) : undefined;
};

const setStoredUser = (user: User) => {
    localStorage.setItem('currentUser', JSON.stringify(user));
};

const clearStoredSession = () => {
    localStorage.clear();
};

const getInitialState = () => ({
    guildId: '',
    gameId: '',
    currentUser: undefined,
});

export const useAppStore = create<AppState>(
    (set) => ({
        guildId: getStoredGuildId(),
        gameId: getStoredGameId(),
        currentUser: getStoredUser(),
        loggedIn: getLoggedIn(),
        setGuildId: (id) => {
            setStoredGuildId(id);
            set(() => ({guildId: id}));
        },
        setGameId: (id) => {
            setStoredGameId(id);
            set(() => ({gameId: id}));
        },
        setCurrentUser: (user) => {
            setStoredUser(user);
            set(() => ({currentUser: user}));
        },
        clearSession: () => {
            clearStoredSession();
            set(() => getInitialState());
        },
        reset: () => {
            clearStoredSession();
            set(() => getInitialState());
        },
    })
);