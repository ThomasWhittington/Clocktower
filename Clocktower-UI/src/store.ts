import {
    create
} from 'zustand';

interface AppState {
    guildId: string,
    currentUserId: string,
    setGuildId: (value: string) => void;
    setCurrentUserId: (value: string) => void;
    isMuted: boolean;
    toggleMute: () => void;

}

export const useAppStore = create<AppState>(
    (set) => ({
        guildId: '',
        currentUserId: '',
        isMuted: false,
        setGuildId: (id) => set(() => ({guildId: id})),
        setCurrentUserId: (id) => set(() => ({currentUserId: id})),
        toggleMute: () => set((state) => ({isMuted: !state.isMuted})),
    }));
