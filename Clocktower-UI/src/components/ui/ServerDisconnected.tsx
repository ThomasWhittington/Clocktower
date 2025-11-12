import {
    Spinner
} from "@/components/ui";
import type {
    HeartbeatProps
} from "@/hooks/useServerHeartbeat.ts";

export const ServerDisconnected = (heartbeatProps: HeartbeatProps) => {
    return (
        <div
            className="flex flex-col items-center justify-center min-h-screen gap-4">
            <Spinner/>
            <p>Server Status: {heartbeatProps.status}</p>
            <p>Last check: {heartbeatProps.lastChecked?.toTimeString()}</p>
            <p>Server time: {heartbeatProps.serverTimestamp?.toTimeString()}</p>
            <button
                onClick={heartbeatProps.manualCheck}
                disabled={heartbeatProps.isChecking}
                className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
                {heartbeatProps.isChecking ? 'Checking...' : 'Check Again'}
            </button>
        </div>
    );
};