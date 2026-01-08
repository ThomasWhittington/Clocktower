import type {MouseEvent, ReactNode} from "react";
import {type User} from "@/types";

export function PlayerNameLabel({player, onClick, children}: Readonly<{
    player: User;
    onClick?: (e: MouseEvent) => void;
    children?: ReactNode;
}>) {
    return (
        <div className="player-name-container">
            <button onClick={onClick}>
                <span className="flex items-center justify-center gap-1">
                    {player.name}
                </span>
            </button>
            {children}
        </div>
    );
}