import {
    useAppStore
} from '@/store';
import {
    resetTownOccupancy
} from "@/components/features/discordTownPanel/hooks";
import {
    resetHubState
} from "@/hooks";

export const resetAllApplicationState = () => {
    useAppStore.getState().reset();
    resetTownOccupancy();
    resetHubState();
};