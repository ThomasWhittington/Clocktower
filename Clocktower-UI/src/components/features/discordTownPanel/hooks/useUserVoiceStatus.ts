import {
    useDiscordHub
} from "@/hooks";

export const useUserVoiceStatus = (userId: string) => {
    const {
        userVoiceStates,
        connectionState
    } = useDiscordHub();

    const voiceStates =
        userId && connectionState === 'Connected'
            ? userVoiceStates[userId] ?? null
            : null;

    return {voiceStates};
};