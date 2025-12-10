import {
    discordService
} from "@/services";
import {
    GameTime
} from "@/types";
import {
    useAppStore
} from "@/store";

export const TimeOfDaySwitch = () => {
    const gameId = useAppStore((state) => state.gameId);
    const setTime = async (gameTime: GameTime) => {
        if (!gameId) return;
        await discordService.setTime(gameId, gameTime)
    }

    return (
        <>
            <button
                onClick={() => setTime(GameTime.Day)}
                className="btn-outline">Day
            </button>
            <button
                onClick={() => setTime(GameTime.Evening)}
                className="btn-outline">Evening
            </button>
            <button
                onClick={() => setTime(GameTime.Night)}
                className="btn-outline">Night
            </button>
        </>
    );
}

