import {PlayerNameLabel} from "@/components/features/townSquare/components";
import type {User} from "@/types";
import {UserAvatar} from "@/components/ui";

export function PlayerIcon({x, y, size, player}: Readonly<{ x: number; y: number; size: number; player: User }>) {
    const isTopHalf = y < 0;
    return (
        <div
            className="absolute top-1/2 left-1/2 flex flex-col items-center gap-1.5 transition-transform duration-700 ease-out"
            style={{transform: `translate(-50%, -50%) translate(${x}px, ${y}px)`}}
        >
            {isTopHalf && (
                <PlayerNameLabel name={player.name}/>
            )}

            <UserAvatar user={player} size={size}/>

            {!isTopHalf && (
                <PlayerNameLabel name={player.name}/>
            )}
        </div>
    );
}