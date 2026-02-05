import type {PlayerActionContext} from "@/components/features/townSquare/config";
import {
    ActionBanner,
    PlayerActionMenu,
    PlayerIcon
} from "@/components/features/townSquare/components";
import {
    HandIcon,
    NoVoteIcon,
    PointIcon,
    SkullIcon,
    SwapIcon
} from "@/components/ui/icons";
import {
    getPlayerGlowColor,
    useCircleLayout
} from "@/components/features/townSquare/hooks";
import {
    useDiscordTown,
    useUser
} from "@/components/features/discordTownPanel/hooks";
import {
    toggleMarkPlayer,
    useElementSize,
    useKeyboardShortcut,
} from "@/hooks";
import {Spinner} from "@/components/ui";
import {useAppStore} from "@/store";
import {
    useEffect,
    useState
} from "react";
import {User} from "@/types";
import {
    AnimatePresence,
    motion
} from "framer-motion";
import {animations} from "@/constants";

interface TownSquareProps {
    showDraftRoles?: boolean;
    onTokenClick?: (player: User) => void;
    onCommitDraftRoles?: () => void;
    onCircleSizeChange?: (diameter: number) => void;
    townSquareActions: ReturnType<typeof import('@/components/features/townSquare/hooks').useTownSquareActions>;
}

export default function TownSquare({
                                       showDraftRoles = false,
                                       onTokenClick,
                                       onCommitDraftRoles,
                                       onCircleSizeChange,
                                       townSquareActions
                                   }: Readonly<TownSquareProps>) {
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const {gameId} = useAppStore();
    const {discordTown, isLoading, error} = useDiscordTown();
    const {currentUser} = useAppStore();
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
        nominatingPlayer,
        initiateNomination,
        confirmNomination,
        cancelNomination,
        playerNominatesPlayer
    } = townSquareActions;

    const {positions, size} = useCircleLayout({
        count: discordTown?.players?.length ?? 0,
        containerWidth: parentSize.width,
        containerHeight: parentSize.height
    });

    const circleDiameter = Math.min(parentSize.width, parentSize.height) - 2 * size;
    useEffect(() => {
        if (onCircleSizeChange && circleDiameter > 0) {
            onCircleSizeChange(circleDiameter);
        }
    }, [circleDiameter, onCircleSizeChange]);

    if (!gameId) return;
    const actionContext: PlayerActionContext = {
        initiateSwap,
        toggleMarkPlayer: player => void toggleMarkPlayer(gameId, player.id),
        initiateNomination,
        playerNominatesPlayer
    };

    return (
        <div ref={containerRef} className="townsquare" onClick={closeMenu}>
            {isLoading && <Spinner/>}
            {error && <p className="error-text">{error}</p>}
            {swappingPlayer && (
                <ActionBanner onCancel={cancelSwap} message={<div>Swapping <span>{swappingPlayer.name}</span>...</div>}/>
            )}

            {nominatingPlayer && (
                <ActionBanner onCancel={cancelNomination} message={<div><span>{nominatingPlayer.name}</span> Nominating...</div>}/>
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
                const isNominatingTarget = nominatingPlayer !== null;

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
                        avatarOverlay={
                            <>
                                {isSwappingTarget &&
                                    <button className="portrait-overlay clickable-portrait" onClick={() => confirmSwap(player)}>
                                        <SwapIcon className="portrait-icon"/>
                                    </button>
                                }
                                {isNominatingTarget &&
                                    <button className="portrait-overlay clickable-portrait" onClick={() => confirmNomination(player)}>
                                        <PointIcon className="portrait-icon"/>
                                    </button>
                                }
                                <AnimatePresence>
                                    {player.isMarked &&
                                        <motion.div key="marked" {...animations.zoomInSpring} className="portrait-overlay">
                                            <SkullIcon className="portrait-icon marked"/>
                                        </motion.div>
                                    }
                                    {player.handUp &&
                                        <motion.div key="handup" {...animations.zoomInSpring} className="portrait-overlay">
                                            <HandIcon className={`portrait-icon hand-up${player.voteLocked ? '' : ' unlocked'}`} gradientId="shared-hand"/>
                                        </motion.div>
                                    }
                                    {!player.handUp && player.voteLocked &&
                                        <motion.div key="noVote" {...animations.zoomInSpring} className="portrait-overlay">
                                            <NoVoteIcon className="portrait-icon no-vote"/>
                                        </motion.div>
                                    }
                                </AnimatePresence>
                            </>
                        }
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