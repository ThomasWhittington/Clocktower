import {Spinner} from "@/components/ui";
import {useTimerActions} from "@/hooks/useTimerActions.ts";
import {
    useMemo,
    useState
} from "react";

function DiscordAdminPanel() {
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
            <div>
                {isTimerLoading &&
                    <Spinner/>}
                {timerResult &&
                    <p className="text-green-500 text-sm">{timerResult}</p>}
                {timerError &&
                    <p className="error-text">{timerError}</p>}

                {timerActionsCanRun &&
                    <>
                        <input type="number" min={1} value={timerSeconds / 60} onChange={(e) => setTimerSeconds(Number(e.target.value) * 60)} placeholder="Minutes" className="input-primary w-24"/>
                        <input type="text" value={timerLabel} onChange={(e) => setTimerLabel(e.target.value)} placeholder="Label (optional)" className="input-primary w-32 placeholder:select-none"/>
                        <button className="btn-primary" aria-label="Start or edit timer" disabled={!canSubmitTimer} onClick={() => startOrEditTimer(timerSeconds, timerLabel.trim() || undefined)}>
                            ⌛
                        </button>
                        <button className="btn-danger" aria-label="Cancel timer" disabled={!timerActionsCanRun || isTimerLoading} onClick={cancelTimer}>
                            ❌
                        </button>
                    </>
                }
            </div>
        </div>

    )
}

export default DiscordAdminPanel;