import type {
    MouseEvent,
    ReactNode
} from "react";
import {type User} from "@/types";

export function PlayerNameLabel({player, onClick, children}: Readonly<{
    player: User;
    onClick?: (e: MouseEvent) => void;
    children?: ReactNode;
}>) {
    return (
        <div className="player-name-container">
            <button className={player.isDead ? 'is-dead' : ''} onClick={onClick}>
                {player.name}
            </button>
            {children}
        </div>
    );
}