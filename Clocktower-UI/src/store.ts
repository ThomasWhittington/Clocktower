import {
    create
} from 'zustand';
import type {
    User
} from "./types/auth.ts";

interface AppState {
    guildId: string,
    currentUser?: User,
    setGuildId: (value: string) => void;
    setCurrentUser: (value: User) => void;
    isMuted: boolean;
    toggleMute: () => void;

}
const getStoredGuildId = (): string => {
    return localStorage.getItem('guildId') || '';
};

const getStoredUser = (): User | undefined => {
    const stored = localStorage.getItem('currentUser');
    return stored ? JSON.parse(stored) : undefined;
};

const getStoredMuteState = (): boolean => {
    return localStorage.getItem('isMuted') === 'true';
};

export const useAppStore = create<AppState>(
    (set) => ({
        guildId: getStoredGuildId(),
        currentUser: getStoredUser(),
        isMuted: getStoredMuteState(),
        setGuildId: (id) => {
            localStorage.setItem('guildId', id);
            set(() => ({guildId: id}));
        },
        setCurrentUser: (user) => {
            localStorage.setItem('currentUser', JSON.stringify(user));
            set(() => ({currentUser: user}));
        },
        toggleMute: () => set((state) => {
            const newMuteState = !state.isMuted;
            localStorage.setItem('isMuted', newMuteState.toString());
            return {isMuted: newMuteState};
        }),
    }));