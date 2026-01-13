import {useEffect} from 'react';

interface UseKeyboardShortcutOptions {
    key: string;
    onKeyPress: () => void;
    ctrlKey?: boolean;
    shiftKey?: boolean;
    altKey?: boolean;
    enabled?: boolean;
}

export const useKeyboardShortcut = ({
                                        key,
                                        onKeyPress,
                                        ctrlKey = false,
                                        shiftKey = false,
                                        altKey = false,
                                        enabled = true
                                    }: UseKeyboardShortcutOptions) => {
    useEffect(() => {
        if (!enabled) return;
        const handleKeyDown = (event: KeyboardEvent) => {
            if (
                event.key.toLowerCase() === key.toLowerCase() &&
                event.ctrlKey === ctrlKey &&
                event.shiftKey === shiftKey &&
                event.altKey === altKey
            ) {
                event.preventDefault();
                onKeyPress();
                if (document.activeElement instanceof HTMLElement) {
                    document.activeElement.blur();
                }
            }
        };

        globalThis.addEventListener('keydown', handleKeyDown);
        return () => globalThis.removeEventListener('keydown', handleKeyDown);
    }, [key, onKeyPress, ctrlKey, shiftKey, altKey]);
};