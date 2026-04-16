import {useTimer} from "@/hooks";
import {
    useEffect,
    useRef
} from "react";

function formatMs(ms: number) {
    const totalSeconds = Math.ceil(ms / 1000);
    const m = Math.floor(totalSeconds / 60);
    const s = totalSeconds % 60;
    return `${m}:${String(s).padStart(2, '0')}`;
}

export const Timer = () => {
    const timer = useTimer();
    const initialRemainingRef = useRef<number | null>(null);
    useEffect(() => {
        if (timer.isRunning && timer.remainingMs != null) {
            if (initialRemainingRef.current === null) {
                initialRemainingRef.current = timer.remainingMs;
            }
        } else {
            initialRemainingRef.current = null;
        }
    }, [timer.isRunning, timer.remainingMs]);

    if (!timer.isRunning || timer.remainingMs == null) {
        return null;
    }

    const isLowTime = timer.remainingMs < 30000;
    const isCriticalTime = timer.remainingMs < 10000;

    const progressPercentage = initialRemainingRef.current && initialRemainingRef.current > 0
        ? Math.max(0, Math.min(100, (timer.remainingMs / initialRemainingRef.current) * 100))
        : 100;

    return (
        <div className="bg-gradient-to-br from-slate-900/90 to-slate-800/90 backdrop-blur-md px-6 py-4 rounded-xl pointer-events-auto text-white shadow-2xl border border-white/10">
            <div className="flex flex-col items-center gap-2">
                <p className="text-xs uppercase tracking-wider text-slate-300 font-semibold">
                    {timer.label ?? 'Timer'}
                </p>
                <div className={`text-4xl font-bold font-mono tabular-nums transition-colors duration-300 ${
                    isCriticalTime
                        ? 'text-red-400 animate-pulse'
                        : isLowTime
                            ? 'text-amber-400'
                            : 'text-emerald-400'
                }`}>
                    {formatMs(timer.remainingMs)}
                </div>
                {/* Progress bar */}
                <div className="w-full h-1.5 bg-slate-700/50 rounded-full overflow-hidden mt-1">
                    <div
                        className={`h-full transition-all duration-300 rounded-full ${
                            isCriticalTime
                                ? 'bg-gradient-to-r from-red-500 to-red-600'
                                : isLowTime
                                    ? 'bg-gradient-to-r from-amber-500 to-amber-600'
                                    : 'bg-gradient-to-r from-emerald-500 to-emerald-600'
                        }`}
                        style={{
                            width: `${progressPercentage}%`,
                            animation: isCriticalTime ? 'pulse 1s ease-in-out infinite' : 'none'
                        }}
                    />
                </div>
            </div>
        </div>
    );
}