import {
    useAppStore
} from '@/store';
import {
    resetDiscordTown
} from "@/components/features/discordTownPanel/hooks";
import {
    resetHubState
} from "@/hooks";

export const resetAllApplicationState = () => {
    useAppStore.getState().reset();
    resetDiscordTown();
    resetHubState();
};