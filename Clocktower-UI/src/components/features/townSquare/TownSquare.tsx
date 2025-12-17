import {useState} from "react";
import {PlayerIcon, TownSquareControls} from "@/components/features/townSquare/components";
import {useCircleLayout} from "@/components/features/townSquare/hooks";
import {useElementSize} from "@/hooks";

export default function TownSquare() {
    const [playerCount, setPlayerCount] = useState(8);
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();

    const {positions: players, size} = useCircleLayout({
        count: playerCount,
        containerWidth: parentSize.width,
        containerHeight: parentSize.height,
    });

    return (
        <div>
            <TownSquareControls
                playerCount={playerCount}
                min={1}
                max={24}
                onPlayerCountChange={setPlayerCount}
            />
            <div ref={containerRef} className="relative w-full h-screen overflow-hidden">
                {players.map(({idx, x, y}) => (
                    <PlayerIcon key={idx} idx={idx} x={x} y={y} size={size}/>
                ))}
            </div>
        </div>
    );
};
