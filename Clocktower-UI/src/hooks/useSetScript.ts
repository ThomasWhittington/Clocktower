import {gamesService} from "@/services";
import {ScriptSelect} from "@/types";
import {useAppStore} from "@/store";
import {useCallback} from "react";

export const useSetScript = () => {
    const {gameId} = useAppStore();
    return useCallback(
        async (scriptSelect: ScriptSelect, json?: string) => {
            if (!gameId) return;
            await gamesService.setScript(gameId, scriptSelect, json);
        },
        [gameId],
    );
};