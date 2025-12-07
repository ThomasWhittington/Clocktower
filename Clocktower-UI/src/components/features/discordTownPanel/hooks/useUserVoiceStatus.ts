import {
    useServerHub
} from "@/hooks";

export const useUserVoiceStatus = (userId: string) => {
    const {
        userVoiceStates,
        connectionState
    } = useServerHub();

    const voiceStates =
        userId && connectionState === 'Connected'
            ? userVoiceStates[userId] ?? null
            : null;

    return {voiceStates};
};