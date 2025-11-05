import {
    useAppStore
} from '@/store';
import {
    resetDiscordHub,
    resetTownOccupancy
} from "@/components/features/discordTownPanel/hooks";

export const resetAllApplicationState = () => {
    useAppStore.getState().reset();
    resetTownOccupancy();
    resetDiscordHub();
};