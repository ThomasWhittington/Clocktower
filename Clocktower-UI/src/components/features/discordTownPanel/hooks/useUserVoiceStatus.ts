import {
    useAppStore
} from '@/store';
import {
    useEffect,
    useState
} from "react";
import {
    useTownOccupancy
} from "./useTownOccupancy.ts";
import {
    TownOccupantsUtils
} from "@/utils";
import {
    updateDiscordHubState,
    useDiscordHub
} from "@/hooks";

export const useUserVoiceStatus = () => {
    const currentUser = useAppStore((state) => state.currentUser);
    const {
        userVoiceStates,
        connectionState
    } = useDiscordHub();
    const {townOccupancy} = useTownOccupancy();

    const [initialCheckDone, setInitialCheckDone] = useState(false);

    useEffect(() => {
        if (!currentUser?.id || !townOccupancy || initialCheckDone) return;
        if (connectionState !== 'Connected') return;
        const userId = currentUser.id.toString();
        const isInVoice = TownOccupantsUtils.containsUser(townOccupancy, userId);

        updateDiscordHubState({
            userVoiceStates: {
                ...userVoiceStates,
                [userId]: isInVoice
            }
        });

        setInitialCheckDone(true);
    }, [currentUser?.id, townOccupancy, connectionState, initialCheckDone, userVoiceStates]);

    const isInVoiceChannel = currentUser?.id
        ? userVoiceStates[currentUser.id.toString()] ?? false
        : false;

    return {isInVoiceChannel};
};