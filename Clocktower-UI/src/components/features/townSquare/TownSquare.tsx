import type {PlayerActionContext} from "@/components/features/townSquare/config";
import {
    ActionBanner,
    PlayerActionMenu,
    PlayerIcon
} from "@/components/features/townSquare/components";
import {SwapIcon} from "@/components/ui/icons";
import {
    getPlayerGlowColor,
    useCircleLayout,
    useTownSquareActions
} from "@/components/features/townSquare/hooks";
import {
    useDiscordTown,
    useUser
} from "@/components/features/discordTownPanel/hooks";
import {
    useElementSize,
    useKeyboardShortcut
} from "@/hooks";
import {Spinner} from "@/components/ui";
import {useAppStore} from "@/store";
import {useState} from "react";
import {User} from "@/types";


interface TownSquareProps {
    showDraftRoles?: boolean;
    onTokenClick?: (player: User) => void;
    onCommitDraftRoles?: () => void;
}

export default function TownSquare({showDraftRoles = false, onTokenClick, onCommitDraftRoles}: Readonly<TownSquareProps>) {
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const {discordTown, isLoading, error} = useDiscordTown();
    const {currentUser, gameId} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const [showToken, setShowToken] = useState<boolean>(true);
    useKeyboardShortcut({key: 'g', onKeyPress: () => setShowToken(prev => !prev)});

    const {
        activeMenuPlayerId,
        swappingPlayer,
        toggleMenu,
        closeMenu,
        initiateSwap,
        confirmSwap,
        cancelSwap,
    } = useTownSquareActions();

    const {positions, size} = useCircleLayout({
        count: discordTown?.players?.length ?? 0,
        containerWidth: parentSize.width,
        containerHeight: parentSize.height
    });

    const actionContext: PlayerActionContext = {
        gameId: gameId ?? "",
        currentUser: thisUser,
        initiateSwap
    };
    return (
        <div ref={containerRef} className="townsquare" onClick={closeMenu}>
            {isLoading && <Spinner/>}
            {error && <p className="error-text">{error}</p>}
            {swappingPlayer && (
                <ActionBanner onCancel={cancelSwap} message={<div>Swapping <span>{swappingPlayer.name}</span>...</div>}/>
            )}
            {showDraftRoles && (
                <div className="draft-mode-indicator">
                    <span>📝 Draft Mode</span>
                    {onCommitDraftRoles && (
                        <button onClick={onCommitDraftRoles} className="btn-danger">
                            Send to Players
                        </button>
                    )}
                </div>
            )}

            {discordTown?.players?.map((player, index) => {
                const pos = positions[index];
                if (!pos) return null;

                const isSwappingTarget = swappingPlayer !== null && swappingPlayer.id !== player.id;

                const glowColor = getPlayerGlowColor({
                    player,
                    currentUser: thisUser,
                    discordTown
                });

                return (
                    <PlayerIcon
                        key={player.id}
                        x={pos.x}
                        y={pos.y}
                        size={size}
                        player={player}
                        glowColor={glowColor}
                        showToken={showToken}
                        showDraftRoles={showDraftRoles}
                        onNameClick={(e) => toggleMenu(player.id, e)}
                        onTokenClick={onTokenClick}
                        avatarOverlay={isSwappingTarget && (
                            <button className="clickable-portrait" onClick={() => confirmSwap(player)}>
                                <SwapIcon className="portrait-icon"/>
                            </button>
                        )}
                    >
                        {activeMenuPlayerId === player.id && (
                            <PlayerActionMenu
                                player={player}
                                context={actionContext}
                            />
                        )}
                    </PlayerIcon>
                );
            })}
        </div>
    );
};