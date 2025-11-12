import {
    useCallback,
    useEffect,
    useRef,
    useState
} from 'react';
import {
    adminService
} from "@/services";

export interface HeartbeatProps {
    status: string;
    lastChecked: Date;
    serverTimestamp: Date;
    isChecking: boolean;
    manualCheck: () => void;
}

export function useServerHeartbeat(url = '/api/health', healthyInterval = 5000, unhealthyInterval = 30000): HeartbeatProps {
    const [status, setStatus] = useState('unknown');
    const [lastChecked, setLastChecked] = useState<Date>(new Date());
    const [serverTimestamp, setServerTimestamp] = useState<Date>(new Date());
    const [isChecking, setIsChecking] = useState(false);
    const timerRef = useRef<number | null>(null);

    const pingServer = useCallback(async () => {
        setIsChecking(true);
        await adminService.health()
            .then(data => {
                const newStatus = data.status || 'healthy';
                setStatus(newStatus);
                setServerTimestamp(new Date(data.timeStamp));
                const nextInterval = newStatus === 'healthy' ? healthyInterval : unhealthyInterval;
                scheduleNextPing(nextInterval);
            })
            .catch(_ => {
                setStatus("unreachable");
                scheduleNextPing(unhealthyInterval);
            })
            .finally(() => {
                setLastChecked(new Date());
                setIsChecking(false);
            });
    }, [healthyInterval, unhealthyInterval]);

    const scheduleNextPing = useCallback((intervalMs: number) => {
        if (timerRef.current) {
            clearTimeout(timerRef.current);
        }

        timerRef.current = setTimeout(() => {
            pingServer();
        }, intervalMs);
    }, [pingServer]);

    const manualCheck = useCallback(() => {
        if (timerRef.current) {
            clearTimeout(timerRef.current);
        }
        pingServer();
    }, [pingServer]);


    useEffect(() => {
        pingServer();

        return () => {
            if (timerRef.current) {
                clearTimeout(timerRef.current);
            }
        };
    }, [url, healthyInterval, unhealthyInterval, pingServer]);

    return {
        status,
        lastChecked,
        serverTimestamp,
        isChecking,
        manualCheck
    };
}