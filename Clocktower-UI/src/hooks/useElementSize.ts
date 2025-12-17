import {useCallback, useLayoutEffect, useState} from "react";

type Size = { width: number; height: number };

export function useElementSize<T extends Element>() {
    const [element, setElement] = useState<T | null>(null);
    const [size, setSize] = useState<Size>({ width: 0, height: 0 });

    const ref = useCallback((node: T | null) => {
        setElement(node);
    }, []);

    useLayoutEffect(() => {
        if (!element) return;

        const update = () => {
            const rect = element.getBoundingClientRect();
            setSize({ width: rect.width, height: rect.height });
        };

        update(); // set initial size immediately

        const observer = new ResizeObserver(([entry]) => {
            const { width, height } = entry.contentRect;
            setSize({ width, height });
        });

        observer.observe(element);
        return () => observer.disconnect();
    }, [element]);

    return { ref, size };
}