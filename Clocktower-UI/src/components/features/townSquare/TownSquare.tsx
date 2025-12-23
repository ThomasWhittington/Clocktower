import {PlayerIcon} from "@/components/features/townSquare/components";
import {useCircleLayout} from "@/components/features/townSquare/hooks";
import {useElementSize} from "@/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {Spinner} from "@/components/ui";

export default function TownSquare() {
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const {
        discordTown,
        isLoading,
        error
    } = useDiscordTown();


    const {positions, size} = useCircleLayout({
        count: discordTown?.players?.length ?? 0,
        containerWidth: parentSize.width,
        containerHeight: parentSize.height,
    });

    return (
        <div>
            {isLoading && <Spinner/>}
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <div ref={containerRef} className="relative w-full h-screen overflow-hidden">
                {discordTown?.players?.map((player, index) => {
                    const pos = positions[index];
                    if (!pos) return null;
                    return <PlayerIcon key={player.id} x={pos.x} y={pos.y} size={size} player={player}/>
                })}
            </div>
        </div>
    );
};
