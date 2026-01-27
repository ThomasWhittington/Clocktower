import {
    AvatarOverlays,
    FlippableAvatar,
    PlayerNameLabel
} from "@/components/features/townSquare/components";
import type {User} from "@/types";
import {
    type CSSProperties,
    type MouseEvent,
    type ReactNode,
    useRef,
    useState
} from "react";
import {
    type ColorKey,
    colors
} from "@/theme";
import {
    AnimatePresence,
    motion
} from "framer-motion";

interface PlayerIconProps {
    x: number;
    y: number;
    size: number;
    player: User;
    onNameClick: (e: MouseEvent) => void;
    avatarOverlay?: ReactNode;
    glowColor?: ColorKey;
    showToken: boolean;
    children?: ReactNode;
    showDraftRoles: boolean;
    onTokenClick?: (player: User) => void;
}

export function PlayerIcon({x, y, size, player, onNameClick, avatarOverlay, glowColor, showToken, children, showDraftRoles, onTokenClick}: Readonly<PlayerIconProps>) {
    const isTopHalf = y < 0;
    const playerIconStyle = {'--player-x': `${x}px`, '--player-y': `${y}px`} as CSSProperties;
    const glowColorStyle = glowColor ? {'--glow-color': colors[glowColor]} as CSSProperties : undefined;
    const role = showDraftRoles ? player.draftRole : player.role;
    const [showTooltip, setShowTooltip] = useState(false);
    const avatarRef = useRef<HTMLDivElement>(null);

    return (
        <div className={`player-icon${showTooltip ? ' has-tooltip' : ''}`} style={playerIconStyle}>
            {isTopHalf &&
                <PlayerNameLabel player={player} onClick={onNameClick}>{children}</PlayerNameLabel>
            }
            <div
                ref={avatarRef}
                className="avatar-container"
                style={glowColorStyle}
                onMouseEnter={() => setShowTooltip(true)}
                onMouseLeave={() => setShowTooltip(false)}
            >
                <FlippableAvatar player={player} size={size} showToken={showToken} showDraftRoles={showDraftRoles} onTokenClick={onTokenClick}/>
                {avatarOverlay}
                <AvatarOverlays player={player}/>
            </div>

            {!isTopHalf && (
                <PlayerNameLabel player={player} onClick={onNameClick}>{children}</PlayerNameLabel>
            )}

            <AnimatePresence>
                {role?.description && showTooltip && (
                    <motion.div
                        className={`role-description-tooltip ${isTopHalf ? 'top-half' : 'bottom-half'}`}
                        initial={{opacity: 0, scale: 0.95}}
                        animate={{opacity: 1, scale: 1}}
                        exit={{opacity: 0, scale: 0.95}}
                        transition={{duration: 0.2, ease: "easeOut"}}
                        style={{
                            x: "-50%",
                        }}
                    >
                        <div className="role-description-content">
                            <strong>{role.name}</strong>
                            <p>{role.description}</p>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}