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
    memo,
    useCallback,
    useEffect,
    useMemo,
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

const TownSquarePlayer = memo(function TownSquarePlayer({
                                                            player,
                                                            pos,
                                                            size,
                                                            showToken,
                                                            showDraftRoles,
                                                            thisUser,
                                                            discordTown,
                                                            swappingPlayer,
                                                            nominatingPlayer,
                                                            activeMenuPlayerId,
                                                            toggleMenu,
                                                            confirmSwap,
                                                            confirmNomination,
                                                            onTokenClick,
                                                            actionContext
                                                        }: {
    player: User;
    pos: { idx: number; x: number; y: number };
    size: number;
    showToken: boolean;
    showDraftRoles: boolean;
    thisUser: User | undefined;
    discordTown: ReturnType<typeof useDiscordTown>['discordTown'];
    swappingPlayer: User | null;
    nominatingPlayer: User | null;
    activeMenuPlayerId: string | null;
    toggleMenu: (playerId: string, e: React.MouseEvent) => void;
    confirmSwap: (player: User) => void;
    confirmNomination: (player: User) => void;
    onTokenClick?: (player: User) => void;
    actionContext: PlayerActionContext;
}) {
    const isSwappingTarget = swappingPlayer !== null && swappingPlayer.id !== player.id;
    const isNominatingTarget = nominatingPlayer !== null;

    const glowColor = useMemo(
        () => getPlayerGlowColor({
            player,
            currentUser: thisUser,
            discordTown
        }),
        [player, thisUser, discordTown]
    );

    return (
        <PlayerIcon
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
                    <AnimatePresence mode="wait">
                        {player.isMarked && (
                            <motion.div
                                key="marked"
                                {...animations.zoomInSpring}
                                className="portrait-overlay"
                            >
                                <SkullIcon className="portrait-icon marked"/>
                            </motion.div>
                        )}
                        {player.handUp && (
                            <motion.div
                                key="handup"
                                {...animations.zoomInSpring}
                                className="portrait-overlay"
                            >
                                <HandIcon className={`portrait-icon hand-up${player.voteLocked ? '' : ' unlocked'}`} gradientId="shared-hand"/>
                            </motion.div>
                        )}
                        {!player.handUp && player.voteLocked && (
                            <motion.div
                                key="noVote"
                                {...animations.zoomInSpring}
                                className="portrait-overlay"
                            >
                                <NoVoteIcon className="portrait-icon no-vote"/>
                            </motion.div>
                        )}
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
});

export default function TownSquare({
                                       showDraftRoles = false,
                                       onTokenClick,
                                       onCommitDraftRoles,
                                       onCircleSizeChange,
                                       townSquareActions
                                   }: Readonly<TownSquareProps>) {
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const [debouncedSize, setDebouncedSize] = useState(parentSize);
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
        requestToTalk,
    } = townSquareActions;

    useEffect(() => {
        const timeoutId = setTimeout(() => {
            setDebouncedSize(parentSize);
        }, 50);

        return () => clearTimeout(timeoutId);
    }, [parentSize.width, parentSize.height]);

    const {positions, size} = useCircleLayout({
        count: discordTown?.players?.length ?? 0,
        containerWidth: debouncedSize.width,
        containerHeight: debouncedSize.height
    });

    const circleDiameter = useMemo(
        () => Math.min(debouncedSize.width, debouncedSize.height) - 2 * size,
        [debouncedSize.width, debouncedSize.height, size]
    );

    useEffect(() => {
        if (!onCircleSizeChange || circleDiameter <= 0) return;

        const timeoutId = setTimeout(() => {
            onCircleSizeChange(circleDiameter);
        }, 100);

        return () => clearTimeout(timeoutId);
    }, [circleDiameter, onCircleSizeChange]);
    const handleToggleMarkPlayer = useCallback((player: User) => {
        if (gameId) {
            void toggleMarkPlayer(gameId, player.id);
        }
    }, [gameId]);
    const actionContext = useMemo<PlayerActionContext>(() => ({
        initiateSwap,
        toggleMarkPlayer: handleToggleMarkPlayer,
        initiateNomination,
        requestToTalk,
    }), [initiateSwap, handleToggleMarkPlayer, initiateNomination, requestToTalk]);

    if (!gameId) return null;

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

                return (
                    <TownSquarePlayer
                        key={player.id}
                        player={player}
                        pos={pos}
                        size={size}
                        showToken={showToken}
                        showDraftRoles={showDraftRoles}
                        thisUser={thisUser}
                        discordTown={discordTown}
                        swappingPlayer={swappingPlayer}
                        nominatingPlayer={nominatingPlayer}
                        activeMenuPlayerId={activeMenuPlayerId}
                        toggleMenu={toggleMenu}
                        confirmSwap={confirmSwap}
                        confirmNomination={confirmNomination}
                        onTokenClick={onTokenClick}
                        actionContext={actionContext}
                    />
                );
            })}
        </div>
    );
};