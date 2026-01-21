import {useEffect} from 'react';
import ReactGA from 'react-ga4';

export const useErrorTracking = () => {
    useEffect(() => {
        const handleError = (event: ErrorEvent) => {
            const sanitizedLabel = `${event.message} - ${event.filename}:${event.lineno}`.substring(0, 100);
            ReactGA.event({
                category: "Error",
                action: "javascript_error",
                label: sanitizedLabel
            });
        };

        const handleRejection = (event: PromiseRejectionEvent) => {
            const reason = String(event.reason).substring(0, 100);
            ReactGA.event({
                category: "Error",
                action: "unhandled_promise_rejection",
                label: reason
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