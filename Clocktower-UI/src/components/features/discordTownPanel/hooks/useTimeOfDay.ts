import {discordService} from "@/services";
import {GameTime} from "@/types";
import {useAppStore} from "@/store";
import {useCallback} from "react";

export const useTimeOfDay = () => {
    const {gameId} = useAppStore();

    return useCallback(
        async (gameTime: GameTime) => {
            if (!gameId) return;
            await discordService.setTime(gameId, gameTime);
        },
        [gameId],
    );
};