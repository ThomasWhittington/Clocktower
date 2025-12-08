import {
    useAppStore
} from '@/store';
import {
    useServerHub
} from "@/hooks";
import {
    findGameUserById
} from "@/types";

export const useUserPresenceStatus = () => {
    const currentUser = useAppStore((state) => state.currentUser);
    const {
        townOccupancy,
        connectionState
    } = useServerHub();

    let isInVoiceChannel;
    if (townOccupancy && currentUser?.id && connectionState === 'Connected') {
        const thisUser = findGameUserById(townOccupancy, currentUser.id);
        if (thisUser === undefined) isInVoiceChannel = false;
        else isInVoiceChannel = thisUser.isPresent;
    }

    return {isInVoiceChannel};
};