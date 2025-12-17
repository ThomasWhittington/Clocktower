import type {ChangeEvent} from "react";

type TownSquareControlsProps = {
    playerCount: number;
    min?: number;
    max?: number;
    onPlayerCountChange: (next: number) => void;
};

export function TownSquareControls({playerCount, min = 1, max = 24, onPlayerCountChange}: Readonly<TownSquareControlsProps>) {
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        const raw = Number(e.target.value);
        const clamped = Number.isFinite(raw) ? Math.min(max, Math.max(min, raw)) : min;
        onPlayerCountChange(clamped);
    };

    return (
        <div className="absolute top-6 left-6 flex gap-2">
            <label htmlFor="playerCount">Player count</label>
            <input id="playerCount" type="number" value={playerCount} min={min} max={max} onChange={handleChange}/>
        </div>
    );
}