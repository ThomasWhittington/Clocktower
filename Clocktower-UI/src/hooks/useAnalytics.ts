import {useButtonTracking} from './useButtonTracking';
import {usePageTracking} from './usePageTracking';
import {useLinkTracking} from "@/hooks/useLinkTracking";
import {useErrorTracking} from "@/hooks/useErrorTracking";

export const useAnalytics = () => {
    usePageTracking();
    useButtonTracking();
    useLinkTracking();
    useErrorTracking();
};