import {PlayerNameLabel} from "@/components/features/townSquare/components";
import type {User} from "@/types";
import {UserAvatar} from "@/components/ui";
import type {CSSProperties, MouseEvent, ReactNode} from "react";
import {type ColorKey, colors} from "@/theme";

export function PlayerIcon({x, y, size, player, onNameClick, avatarOverlay, glowColor, children}: Readonly<{
    x: number;
    y: number;
    size: number;
    player: User;
    onNameClick: (e: MouseEvent) => void;
    avatarOverlay?: ReactNode;
    glowColor?: ColorKey;
    children?: ReactNode;
}>) {
    const isTopHalf = y < 0;
    const playerIconStyle = {'--player-x': `${x}px`, '--player-y': `${y}px`} as CSSProperties;
    const glowColorStyle = glowColor ? {'--glow-color': colors[glowColor]} as CSSProperties : undefined;


    return (
        <div className="player-icon" style={playerIconStyle}>
            {isTopHalf && (
                <PlayerNameLabel name={player.name} onClick={onNameClick}>{children}</PlayerNameLabel>
            )}

            <div className="avatar-container" style={glowColorStyle}>
                <UserAvatar user={player} size={size}/>
                {avatarOverlay}
            </div>

            {!isTopHalf && (
                <PlayerNameLabel name={player.name} onClick={onNameClick}>{children}</PlayerNameLabel>
            )}
        </div>
    );
}