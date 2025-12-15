import {
    useEffect,
    useMemo,
    useState
} from 'react';
import {
    type TimerState,
    useServerHub
} from "@/hooks/useServerHub.ts";
import {
    TimerStatus
} from "@/types";


const parseUtcMs = (iso?: string | null) => (iso ? Date.parse(iso) : Number.NaN);

export type TimerView = {
    status: TimerState['status'] | 'None';
    remainingMs: number | null;
    isRunning: boolean;
    label?: string | null;
};

export function useTimer(): TimerView {

    const {timer} = useServerHub();
    const baseline = useMemo(() => {
        if (!timer) {
            return {
                status: 'None' as const,
                remainingMs: null,
                isRunning: false,
                endUtcMs: null,
                offsetMs: 0,
                label: undefined
            };
        }

        const serverNowMs = parseUtcMs(timer.serverNowUtc);
        const endUtcMs = parseUtcMs(timer.endUtc ?? null);

        const offsetMs = Number.isFinite(serverNowMs) ? (serverNowMs - Date.now()) : 0;

        const isRunning = timer.status === TimerStatus.Running && Number.isFinite(endUtcMs);

        const remainingMs = isRunning
            ? Math.max(0, endUtcMs - (Date.now() + offsetMs))
            : null;

        return {
            status: timer.status,
            remainingMs,
            isRunning,
            endUtcMs: Number.isFinite(endUtcMs) ? endUtcMs : null,
            offsetMs,
            label: timer.label
        };
    }, [timer?.status, timer?.serverNowUtc, timer?.endUtc]);

    const [remainingMs, setRemainingMs] = useState<number | null>(baseline.remainingMs);

    useEffect(() => {
        setRemainingMs(baseline.remainingMs);
        if (!baseline.isRunning || baseline.endUtcMs == null) return;

        const id = globalThis.setInterval(() => {
            const next = Math.max(0, baseline.endUtcMs! - (Date.now() + baseline.offsetMs));
            setRemainingMs(next);
        }, 250);

        return () => globalThis.clearInterval(id);
    }, [baseline.isRunning, baseline.endUtcMs, baseline.offsetMs, baseline.remainingMs]);

    return {
        status: baseline.status,
        remainingMs,
        isRunning: baseline.isRunning,
        label: baseline.label
    };
}