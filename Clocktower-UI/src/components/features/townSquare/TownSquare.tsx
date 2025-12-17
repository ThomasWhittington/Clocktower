import {useEffect, useRef, useState} from "react";
import {PlayerIcon} from "@/components/features/townSquare/components";
import {useCircleLayout} from "@/components/features/townSquare/hooks";

type TownSquareProps = {};

export default function TownSquare({}: TownSquareProps) {
    const [playerCount, setPlayerCount] = useState(8);
    const [parentSize, setParentSize] = useState({width: 0, height: 0});
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!containerRef.current) return;
        const observer = new ResizeObserver(([entry]) => {
            const {width, height} = entry.contentRect;
            setParentSize({width, height});
        });
        observer.observe(containerRef.current);
        return () => observer.disconnect();
    }, []);

    const {positions: players, size} = useCircleLayout({count: playerCount, containerWidth: parentSize.width, containerHeight: parentSize.height,});


    return (
        <div>
            <div
                className="absolute top-6 left-6 flex gap-2">
                <label
                    htmlFor="playerCount">Player count</label>
                <input id="playerCount" type="number" value={playerCount} min={1} max={24}
                       onChange={(e) => setPlayerCount(Number(e.target.value))}/>
            </div>
            <div
                ref={containerRef}
                className="relative w-full h-screen overflow-hidden">
                {players.map(({idx, x, y}) => (
                    <PlayerIcon key={idx} idx={idx} x={x} y={y} size={size}/>
                ))}
            </div>
        </div>
    );
};
