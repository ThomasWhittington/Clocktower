import {ActionBanner, PlayerActionMenu, PlayerIcon} from "@/components/features/townSquare/components";
import {useCircleLayout, useTownSquareActions} from "@/components/features/townSquare/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {useElementSize} from "@/hooks";
import {Spinner} from "@/components/ui";

export default function TownSquare() {
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const {discordTown, isLoading, error} = useDiscordTown();

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
        containerHeight: parentSize.height,
    });

    return (
        <div ref={containerRef} className="townsquare" onClick={closeMenu}>
            {isLoading && <Spinner/>}
            {error && <p className="error-text">{error}</p>}
            {swappingPlayer && (
                <ActionBanner onCancel={cancelSwap} message={<div>Swapping <span>{swappingPlayer.name}</span>...</div>}/>
            )}
            {discordTown?.players?.map((player, index) => {
                const pos = positions[index];
                if (!pos) return null;

                const isSwappingTarget = swappingPlayer !== null && swappingPlayer.id !== player.id;

                return (
                    <PlayerIcon
                        key={player.id}
                        x={pos.x}
                        y={pos.y}
                        size={size}
                        player={player}
                        onNameClick={(e) => toggleMenu(player.id, e)}
                        avatarOverlay={isSwappingTarget && (
                            <button className="clickable-portrait" onClick={() => confirmSwap(player)}>
                                <span className="portrait-icon">🔄</span>
                            </button>
                        )}
                    >
                        {activeMenuPlayerId === player.id && (
                            <PlayerActionMenu playerName={player.name}>
                                <button className="player-action-menu-item" onClick={() => initiateSwap(player)}>
                                    <span>🔄</span> Swap Seats
                                </button>
                            </PlayerActionMenu>
                        )}
                    </PlayerIcon>
                );
            })}
        </div>
    );
};