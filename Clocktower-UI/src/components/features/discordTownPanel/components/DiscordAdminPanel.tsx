import {Spinner} from "@/components/ui";
import {useDiscordActions,} from "@/components/features/discordTownPanel/hooks";
import {useTimerActions} from "@/hooks/useTimerActions.ts";
import {useMemo, useState} from "react";
import {TimeOfDaySwitch} from "@/components/features/discordTownPanel/components/TimeOfDaySwitch.tsx";

function DiscordAdminPanel() {
    const {
        sendToCottages,
        sendToTownSquare,
        error,
        result,
        isLoading,
        canRun: discordActionsCanRun
    } = useDiscordActions();
    const {
        startOrEditTimer,
        cancelTimer,
        isLoading: isTimerLoading,
        canRun: timerActionsCanRun,
        error: timerError,
        result: timerResult
    } = useTimerActions();

    const [timerSeconds, setTimerSeconds] = useState<number>(300);
    const [timerLabel, setTimerLabel] = useState<string>("");

    const canSubmitTimer = useMemo(() => {
        return timerActionsCanRun && !isTimerLoading && Number.isFinite(timerSeconds) && timerSeconds > 0;
    }, [timerActionsCanRun, isTimerLoading, timerSeconds]);

    return (
        <div className="flex flex-col space-y-2">
            {isLoading &&
                <Spinner/>}
            {result &&
                <p className="text-green-500 text-sm">{result}</p>}
            {error &&
                <p className="text-red-500 text-sm">{error}</p>}
            <div>
                {discordActionsCanRun &&
                    <>
                        <button className="btn-primary" aria-label="Send to Town Square" onClick={sendToTownSquare}>
                            ⛲
                        </button>
                        <button className="btn-secondary" aria-label="Send to Cottages" onClick={sendToCottages}>
                            🛌
                        </button>
                    </>
                }
                {isTimerLoading &&
                    <Spinner/>}
                {timerResult &&
                    <p className="text-green-500 text-sm">{timerResult}</p>}
                {timerError &&
                    <p className="text-red-500 text-sm">{timerError}</p>}

                {timerActionsCanRun &&
                    <>
                        <input type="number" min={1} value={timerSeconds} onChange={(e) => setTimerSeconds(Number(e.target.value))} placeholder="Seconds" className="input-primary w-24"/>
                        <input type="text" value={timerLabel} onChange={(e) => setTimerLabel(e.target.value)} placeholder="Label (optional)" className="input-primary w-32"/>
                        <button className="btn-primary" aria-label="Start or edit timer" disabled={!canSubmitTimer} onClick={() => startOrEditTimer(timerSeconds, timerLabel.trim() || undefined)}>
                            ⌛
                        </button>
                        <button className="btn-danger" aria-label="Cancel timer" disabled={!timerActionsCanRun || isTimerLoading} onClick={cancelTimer}>
                            ❌
                        </button>
                        <TimeOfDaySwitch/>
                    </>
                }
            </div>
        </div>

    )
}

export default DiscordAdminPanel;