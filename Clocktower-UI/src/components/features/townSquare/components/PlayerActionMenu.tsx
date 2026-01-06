import type {ReactNode} from "react";

interface PlayerActionMenuProps {
    playerName: string;
    children: ReactNode;
}

export function PlayerActionMenu({playerName, children}: Readonly<PlayerActionMenuProps>) {
    return (
        <div className="player-action-menu" onPointerDown={(e) => e.stopPropagation()}>
            <div className="player-action-menu-header">
                <p className="descriptor">Player Actions</p>
                <p className="title">{playerName}</p>
            </div>
            <div className="player-action-menu-items">
                {children}
            </div>
        </div>
    );
}