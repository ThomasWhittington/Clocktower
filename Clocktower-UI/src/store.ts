import {
    create
} from 'zustand';
import type {
    User
} from "./types/auth.ts";

interface AppState {
    loggedIn: boolean,
    guildId: string,
    currentUser?: User,
    setGuildId: (value: string) => void;
    setCurrentUser: (value: User) => void;
    clearSession: () => void;
    reset: () => void;
}

const getLoggedIn = (): boolean => {
    var currentUser = getStoredUser();
    return currentUser != undefined;
}

const getStoredGuildId = (): string => {
    return localStorage.getItem('guildId') || '';
};

const setStoredGuildId = (id: string) => {
    localStorage.setItem('guildId', id);
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
    currentUser: undefined,
});

export const useAppStore = create<AppState>(
    (set) => ({
        guildId: getStoredGuildId(),
        currentUser: getStoredUser(),
        loggedIn: getLoggedIn(),
        setGuildId: (id) => {
            setStoredGuildId(id);
            set(() => ({guildId: id}));
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