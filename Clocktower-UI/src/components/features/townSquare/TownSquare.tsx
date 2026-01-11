import type {PlayerActionContext} from "@/components/features/townSquare/config";
import {ActionBanner, PlayerActionMenu, PlayerIcon} from "@/components/features/townSquare/components";
import {getPlayerGlowColor, useCircleLayout, useTownSquareActions} from "@/components/features/townSquare/hooks";
import {useDiscordTown, useUser} from "@/components/features/discordTownPanel/hooks";
import {useElementSize} from "@/hooks";
import {Spinner} from "@/components/ui";
import {useAppStore} from "@/store";
import {useState} from "react";

export default function TownSquare() {
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const {discordTown, isLoading, error} = useDiscordTown();
    const {currentUser, gameId} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const [showToken, setShowToken] = useState<boolean>(true);
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

            <button className="btn-primary" onClick={() => setShowToken(!showToken)}>Toggle tokens</button>
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
                        onNameClick={(e) => toggleMenu(player.id, e)}
                        avatarOverlay={isSwappingTarget && (
                            <button className="clickable-portrait" onClick={() => confirmSwap(player)}>
                                <span className="portrait-icon">🔄</span>
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