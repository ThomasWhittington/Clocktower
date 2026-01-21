import {useEffect} from 'react';
import ReactGA from 'react-ga4';
import {useAppStore} from '@/store';

export const useUserTracking = () => {
    const currentUser = useAppStore((state) => state.currentUser);

    useEffect(() => {
        if (currentUser?.id) {
            ReactGA.set({
                userId: currentUser.id,
            });
        } else {
            ReactGA.set({userId: null});
        }
    }, [currentUser]);
};