import {useTimer} from "@/hooks";

function formatMs(ms: number) {
    const totalSeconds = Math.ceil(ms / 1000);
    const m = Math.floor(totalSeconds / 60);
    const s = totalSeconds % 60;
    return `${m}:${String(s).padStart(2, '0')}`;
}

export const Timer = () => {
    const timer = useTimer();

    return (
        <>
            {timer.isRunning && timer.remainingMs != null && (
                <div className="bg-black/50 backdrop-blur-sm p-3 rounded-lg pointer-events-auto text-white">
                    <p className="text-sm">
                        {timer.label ?? 'Timer'}: <span className="font-mono">{formatMs(timer.remainingMs)}</span>
                    </p>
                </div>
            )}
        </>
    );
}