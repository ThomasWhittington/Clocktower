import {useEffect} from 'react';

interface UseKeyboardShortcutOptions {
    key: string;
    onKeyPress: () => void;
    ctrlKey?: boolean;
    shiftKey?: boolean;
    altKey?: boolean;
    enabled?: boolean;
}

const isEditableElement = (target: HTMLElement | null): boolean => {
    return target?.tagName === 'INPUT' ||
        target?.tagName === 'TEXTAREA' ||
        target?.contentEditable === 'true';
};
const isKeyMatch = (
    event: KeyboardEvent,
    key: string,
    ctrlKey: boolean,
    shiftKey: boolean,
    altKey: boolean
): boolean => {
    return event.key.toLowerCase() === key.toLowerCase() &&
        event.ctrlKey === ctrlKey &&
        event.shiftKey === shiftKey &&
        event.altKey === altKey;
};
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
            if (isEditableElement(event.target as HTMLElement)) {
                return;
            }

            if (isKeyMatch(event, key, ctrlKey, shiftKey, altKey)) {
                event.preventDefault();
                onKeyPress();
            }
        };

        globalThis.addEventListener('keydown', handleKeyDown);
        return () => globalThis.removeEventListener('keydown', handleKeyDown);
    }, [key, onKeyPress, ctrlKey, shiftKey, altKey, enabled]);
};