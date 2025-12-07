import {
    useAppStore
} from '@/store';
import {
    useServerHub
} from "@/hooks";

export const useUserPresenceStatus = () => {
    const currentUser = useAppStore((state) => state.currentUser);
    const { userPresenceStates, connectionState } = useServerHub();

    const isInVoiceChannel =
        currentUser?.id && connectionState === 'Connected'
            ? userPresenceStates[currentUser.id.toString()] ?? false
            : false;

    return { isInVoiceChannel };
};