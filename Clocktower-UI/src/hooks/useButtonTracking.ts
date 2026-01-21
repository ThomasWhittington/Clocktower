import {useEffect} from 'react';
import ReactGA from 'react-ga4';

export const useButtonTracking = () => {
    useEffect(() => {
        const handleClick = (event: MouseEvent) => {
            const target = event.target as HTMLElement;
            const button = target.closest('button');

            if (button) {
                const buttonText = button.textContent?.trim() || 'Unknown';
                const dataAction = button.dataset.action;

                ReactGA.event({
                    category: "Button Click",
                    action: dataAction || buttonText.toLowerCase().replaceAll(/\s+/g, '_'),
                    label: globalThis.location.pathname
                });
            }
        };

        document.addEventListener('click', handleClick);
        return () => document.removeEventListener('click', handleClick);
    }, []);
};