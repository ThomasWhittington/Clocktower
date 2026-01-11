import {AvatarOverlays, FlippableAvatar, PlayerNameLabel} from "@/components/features/townSquare/components";
import type {User} from "@/types";
import {type CSSProperties, type MouseEvent, type ReactNode} from "react";
import {type ColorKey, colors} from "@/theme";

export function PlayerIcon({x, y, size, player, onNameClick, avatarOverlay, glowColor, showToken, children}: Readonly<{
    x: number;
    y: number;
    size: number;
    player: User;
    onNameClick: (e: MouseEvent) => void;
    avatarOverlay?: ReactNode;
    glowColor?: ColorKey;
    showToken: boolean,
    children?: ReactNode;
}>) {
    const isTopHalf = y < 0;
    const playerIconStyle = {'--player-x': `${x}px`, '--player-y': `${y}px`} as CSSProperties;
    const glowColorStyle = glowColor ? {'--glow-color': colors[glowColor]} as CSSProperties : undefined;

    return (
        <div className="player-icon" style={playerIconStyle}>
            {isTopHalf &&
                <PlayerNameLabel player={player} onClick={onNameClick}>{children}</PlayerNameLabel>
            }
            {player.role && <div className="role-badge">{player.role.name}</div>}
            <div className="avatar-container" style={glowColorStyle}>
                <FlippableAvatar player={player} size={size} showToken={showToken}/>
                {avatarOverlay}
                <AvatarOverlays player={player}/>
            </div>

            {!isTopHalf && (
                <PlayerNameLabel player={player} onClick={onNameClick}>{children}</PlayerNameLabel>
            )}
        </div>
    );
}