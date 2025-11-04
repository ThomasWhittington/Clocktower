import {
    useAppStore
} from '../../../../store';
import {
    useDiscordHub
} from './useDiscordHub';

export const useUserVoiceStatus = () => {
    const currentUser = useAppStore((state) => state.currentUser);
    const {userVoiceStates} = useDiscordHub();

    const isInVoiceChannel = currentUser?.id ? userVoiceStates[currentUser.id.toString()] ?? false : false;

    return {isInVoiceChannel};
};