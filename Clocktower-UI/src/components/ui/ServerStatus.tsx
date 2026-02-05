import {useServerHeartbeat} from '@/hooks/useServerHeartbeat';
import {useServerHub} from '@/hooks/useServerHub';
import {HubConnectionState} from '@microsoft/signalr';

export function ServerStatus() {
    const {status, lastChecked, isChecking, manualCheck} = useServerHeartbeat();
    const {connectionState} = useServerHub();

    const isHttpHealthy = status === 'Healthy';
    const isSignalRConnected = connectionState === HubConnectionState.Connected;
    const isFullyConnected = isHttpHealthy && isSignalRConnected;

    const getStatusColor = () => {
        if (isFullyConnected) return 'text-green-500';
        if (connectionState === HubConnectionState.Reconnecting) return 'text-yellow-500';
        return 'text-red-500';
    };

    const getStatusText = () => {
        if (isFullyConnected) return 'Connected';
        if (connectionState === HubConnectionState.Reconnecting) return 'Reconnecting...';
        if (!isHttpHealthy) return 'Server Unavailable';
        return 'Disconnected';
    };

    const getStatusIndicator = () => {
        if (isChecking) return '○';
        if (isFullyConnected) return '●';
        if (connectionState === HubConnectionState.Reconnecting) return '◐';
        return '○';
    };

    return (
        <div className="relative group select-none">
            <span className={`text-xl ${getStatusColor()} cursor-help`}>
                {getStatusIndicator()}
            </span>

            <div className="absolute right-0 top-full pt-2 invisible group-hover:visible opacity-0 group-hover:opacity-100 transition-opacity duration-200 z-50">
                <div className="bg-gray-800 text-white rounded-lg shadow-lg p-3 min-w-max">
                    <div className="flex items-center gap-2 mb-2">
                        <span className={getStatusColor()}>
                            {getStatusText()}
                        </span>
                    </div>

                    <div className="text-xs text-gray-300 space-y-1">
                        <div>HTTP: {status}</div>
                        <div>SignalR: {connectionState}</div>
                        <div>Last checked: {lastChecked.toLocaleTimeString()}</div>
                    </div>

                    {!isFullyConnected && !isChecking && (
                        <button
                            onClick={manualCheck}
                            className="mt-2 w-full text-xs px-2 py-1 bg-gray-700 hover:bg-gray-600 rounded transition-colors"
                        >
                            Retry Connection
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
}