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
        discordTown,
        connectionState
    } = useServerHub();

    let isInVoiceChannel;
    if (discordTown && currentUser?.id && connectionState === 'Connected') {
        const thisUser = findGameUserById(discordTown, currentUser.id);
        if (thisUser === undefined) isInVoiceChannel = false;
        else isInVoiceChannel = thisUser.isPresent;
    }

    return {isInVoiceChannel};
};