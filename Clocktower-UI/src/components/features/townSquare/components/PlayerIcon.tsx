import {PlayerNameLabel} from "@/components/features/townSquare/components";

export function PlayerIcon({idx, x, y, size}: Readonly<{ idx: number; x: number; y: number; size: number }>) {
    const isTopHalf = y < 0;
    const playerName = `Player ${idx}`;
    const image = "/images/evening-bg.png";
    return (
        <div
            className="absolute top-1/2 left-1/2 flex flex-col items-center gap-1.5 transition-transform duration-700 ease-out"
            style={{transform: `translate(-50%, -50%) translate(${x}px, ${y}px)`}}
        >
            {isTopHalf && (
                <PlayerNameLabel name={playerName}/>
            )}

            <div
                className="rounded-full">
                <img className="object-cover rounded-full" src={image} alt={playerName} style={{width: size, height: size}}/>
            </div>

            {!isTopHalf && (
                <PlayerNameLabel name={playerName}/>
            )}
        </div>
    );
}