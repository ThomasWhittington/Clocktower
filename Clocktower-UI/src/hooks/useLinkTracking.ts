import {useEffect} from 'react';
import ReactGA from 'react-ga4';

export const useLinkTracking = () => {
    useEffect(() => {
        const handleClick = (event: MouseEvent) => {
            const target = event.target as HTMLElement;
            const link = target.closest('a');

            if (link?.href) {
                const isExternal = !link.href.startsWith(globalThis.location.origin);

                ReactGA.event({
                    category: isExternal ? "External Link" : "Internal Link",
                    action: "click",
                    label: link.href
                });
            }
        };

        document.addEventListener('click', handleClick);
        return () => document.removeEventListener('click', handleClick);
    }, []);
};