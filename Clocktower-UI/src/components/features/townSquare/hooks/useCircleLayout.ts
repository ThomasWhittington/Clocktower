import {useMemo} from "react";

interface CircleLayoutOptions {
    count: number;
    containerWidth: number;
    containerHeight: number;
    itemSize?: number;
}

export function useCircleLayout({count, containerWidth, containerHeight, itemSize,}: CircleLayoutOptions) {
    return useMemo(() => {
        if (count <= 0 || containerWidth === 0 || containerHeight === 0) return {
            positions: [],
            size: 0
        };

        const dynamicSize = itemSize ?? Math.min(containerWidth, containerHeight) / 10;

        const radius = Math.min(containerWidth, containerHeight) / 2 - dynamicSize;

        const positions = Array.from({length: count}, (_, i) => {
            const angle = (i / count) * 2 * Math.PI - Math.PI / 2;
            const x = Math.cos(angle) * radius;
            const y = Math.sin(angle) * radius;
            return {idx: i + 1, x, y};
        });

        return {
            positions,
            size: dynamicSize
        };
    }, [count, containerWidth, containerHeight, itemSize]);
}
