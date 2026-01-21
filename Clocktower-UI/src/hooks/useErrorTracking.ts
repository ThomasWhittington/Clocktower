import {useEffect} from 'react';
import ReactGA from 'react-ga4';

export const useErrorTracking = () => {
    useEffect(() => {
        const handleError = (event: ErrorEvent) => {
            ReactGA.event({
                category: "Error",
                action: "javascript_error",
                label: `${event.message} - ${event.filename}:${event.lineno}`
            });
        };

        const handleRejection = (event: PromiseRejectionEvent) => {
            ReactGA.event({
                category: "Error",
                action: "unhandled_promise_rejection",
                label: String(event.reason)
            });
        };

        globalThis.addEventListener('error', handleError);
        globalThis.addEventListener('unhandledrejection', handleRejection);

        return () => {
            globalThis.removeEventListener('error', handleError);
            globalThis.removeEventListener('unhandledrejection', handleRejection);
        };
    }, []);
};