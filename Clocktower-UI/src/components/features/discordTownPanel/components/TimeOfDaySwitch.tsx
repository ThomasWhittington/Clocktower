import {GameTime} from "@/types";
import {useTimeOfDay} from "@/components/features/discordTownPanel/hooks";

export const TimeOfDaySwitch = () => {
    const setTime = useTimeOfDay();
    return (
        <>
            <button onClick={() => setTime(GameTime.Day)} className="btn-outline">
                Day
            </button>
            <button onClick={() => setTime(GameTime.Evening)} className="btn-outline">
                Evening
            </button>
            <button onClick={() => setTime(GameTime.Night)} className="btn-outline">
                Night
            </button>
        </>
    );
}

