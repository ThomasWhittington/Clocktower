import {create} from 'zustand';
import type {User} from "./types/auth.ts";

interface AppState {
    loggedIn: boolean,
    guildId: string,
    gameId: string | null,
    joinedGameId: string | null,
    currentUser?: User,
    paperNotes: Record<string, string>;
    jwt: string | null;
    setGuildId: (value: string) => void;
    setGameId: (value: string | null) => void;
    setJoinedGameId: (value: string | null) => void;
    setCurrentUser: (value: User) => void;
    setJwt: (value: string | undefined) => void;
    setPaperNote: (gameId: string, content: string) => void;
    getPaperNote: (gameId: string) => string;
    clearSession: () => void;
    reset: () => void;
}

const getStoredJwt = (): string | null => {
    return localStorage.getItem('jwt') ?? null;
};

const setStoredJwt = (jwt: string | undefined) => {
    if (jwt) {
        localStorage.setItem('jwt', jwt);
    } else {
        localStorage.removeItem('jwt');
    }
};

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

const getStoredJoinedGameId = (): string => {
    return localStorage.getItem('joinedGameId') || '';
};

const setStoredGuildId = (id: string) => {
    localStorage.setItem('guildId', id);
};

const setStoredGameId = (id: string | null) => {
    if (id) {
        localStorage.setItem('gameId', id);
    } else {
        localStorage.removeItem('gameId');
    }
};

const setStoredJoinedGameId = (id: string | null) => {
    if (id) {
        localStorage.setItem('joinedGameId', id);
    } else {
        localStorage.removeItem('joinedGameId');
    }
};

const getStoredPaperNotes = (): Record<string, string> => {
    const stored = localStorage.getItem('paperNotes');
    if (!stored) return {};
    try {
        return JSON.parse(stored) as Record<string, string>;
    } catch {
        localStorage.removeItem('paperNotes');
        return {};
    }
};

const setStoredPaperNotes = (notes: Record<string, string>) => {
    localStorage.setItem('paperNotes', JSON.stringify(notes));
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
    jwt: undefined,
    paperNotes: {}
});

export const useAppStore = create<AppState>(
    (set, get) => ({
        guildId: getStoredGuildId(),
        gameId: getStoredGameId(),
        joinedGameId: getStoredJoinedGameId(),
        jwt: getStoredJwt(),
        currentUser: getStoredUser(),
        loggedIn: getLoggedIn(),
        paperNotes: getStoredPaperNotes(),
        setGuildId: (id) => {
            setStoredGuildId(id);
            set(() => ({guildId: id}));
        },
        setGameId: (id) => {
            setStoredGameId(id);
            set(() => ({gameId: id}));
        },
        setJoinedGameId: (id) => {
            setStoredJoinedGameId(id);
            set(() => ({joinedGameId: id}));
        },
        setCurrentUser: (user) => {
            setStoredUser(user);
            set(() => ({currentUser: user}));
        },
        setJwt: (jwt) => {
            setStoredJwt(jwt);
            set(() => ({jwt}));
        },
        setPaperNote: (gameId, content) => {
            const notes = {...get().paperNotes, [gameId]: content};
            setStoredPaperNotes(notes);
            set({paperNotes: notes});
        },
        getPaperNote: (gameId) => {
            return get().paperNotes[gameId] || '';
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
