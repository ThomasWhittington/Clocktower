import {useEffect, useRef, useState} from "react";

export const useDropdown = () => {
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handlePointerDown = (event: PointerEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape") setIsOpen(false);
        };
        if (isOpen) {
            document.addEventListener('pointerdown', handlePointerDown);
            document.addEventListener('keydown', handleKeyDown);
        }

        return () => {
            document.removeEventListener('pointerdown', handlePointerDown);
            document.removeEventListener('keydown', handleKeyDown);
        };
    }, [isOpen]);

    const toggle = () => setIsOpen(prev => !prev);
    const close = () => setIsOpen(false);

    return {isOpen, toggle, close, dropdownRef};
};