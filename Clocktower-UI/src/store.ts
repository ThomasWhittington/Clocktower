import {
    create
} from 'zustand';

interface AppState {
    guildId: bigint,
    currentUserId?: bigint,
    setGuildId: (value: bigint) => void;
    setCurrentUserId: (value: bigint) => void;
    isMuted: boolean;
    toggleMute: () => void;

}

export const useAppStore = create<AppState>(
    (set) => ({
        guildId: 0n,
        currentUserId: undefined,
        isMuted: false,
        setGuildId: (id) => set(() => ({guildId: id})),
        setCurrentUserId: (id) => set(() => ({currentUserId: id})),
        toggleMute: () => set((state) => ({isMuted: !state.isMuted})),
    }));
