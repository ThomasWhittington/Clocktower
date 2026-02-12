import type {PlayerActionContext} from "@/components/features/townSquare/config";
import {
    PlayerActionMenu,
    PlayerIcon,
    ReminderBlock
} from "@/components/features/townSquare/components";
import {
    HandIcon,
    NoVoteIcon,
    PointIcon,
    SkullIcon,
    SwapIcon
} from "@/components/ui/icons";
import {getPlayerGlowColor} from "@/components/features/townSquare/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {
    memo,
    type MouseEvent,
    useMemo
} from "react";
import {User} from "@/types";
import {
    AnimatePresence,
    motion
} from "framer-motion";
import {animations} from "@/constants";

interface TownSquarePlayerProps {
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
    toggleMenu: (playerId: string, e: MouseEvent) => void;
    confirmSwap: (player: User) => void;
    confirmNomination: (player: User) => void;
    onTokenClick?: (player: User) => void;
    actionContext: PlayerActionContext;
}

export const TownSquarePlayer = memo(
    function TownSquarePlayer({
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
                              }: TownSquarePlayerProps) {
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

        const angleToCenter = useMemo(() => {
            return Math.atan2(-pos.y, -pos.x) * (180 / Math.PI);
        }, [pos.x, pos.y]);

        const distanceToCenter = useMemo(() => {
            return Math.hypot(pos.x, pos.y);
        }, [pos.x, pos.y]);

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

                        {player.reminderTokens.length > 0 &&
                            <ReminderBlock
                                reminderTokens={player.reminderTokens}
                                distanceToCenter={distanceToCenter}
                                angleToCenter={angleToCenter}
                                size={size}
                            />
                        }
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
        )
            ;
    }
);