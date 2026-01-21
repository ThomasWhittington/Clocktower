import {useButtonTracking} from './useButtonTracking';
import {usePageTracking} from './usePageTracking';
import {useLinkTracking} from "@/hooks/useLinkTracking.ts";
import {useErrorTracking} from "@/hooks/useErrorTracking.ts";

export const useAnalytics = () => {
    usePageTracking();
    useButtonTracking();
    useLinkTracking();
    useErrorTracking();
};