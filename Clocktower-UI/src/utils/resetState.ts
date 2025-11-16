import {
    useAppStore
} from '@/store';
import {
    resetTownOccupancy
} from "@/components/features/discordTownPanel/hooks";
import {
    resetDiscordHub
} from "@/hooks";

export const resetAllApplicationState = () => {
    useAppStore.getState().reset();
    resetTownOccupancy();
    resetDiscordHub();
};