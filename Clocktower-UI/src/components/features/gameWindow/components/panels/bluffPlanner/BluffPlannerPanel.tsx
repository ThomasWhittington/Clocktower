import {BasePanel} from "@/components/ui";
import {type User} from "@/types";
import {
    useElementSize,
    useServerHub
} from "@/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {useBluffPlanner} from "@/components/features/gameWindow/hooks";
import {TeamGroup} from "@/components/features/gameWindow/components/panels/rolePlanner";
import {Token} from "@/components/tokens";

interface BluffPlannerPanelProps {
    isOpen: boolean;
    onClose: () => void;
    player: User;
}

export const BluffPlannerPanel = ({isOpen, onClose, player}: BluffPlannerPanelProps) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const dynamicSize = Math.min(parentSize.width, parentSize.height) / 8 || 64;

    const {
        slots,
        selectedRoleIds,
        pendingRole,
        handleRoleClick,
        handleSlotClick
    } = useBluffPlanner({player});

    const inPlayRoleIds = new Set(
        (discordTown?.players ?? [])
            .flatMap(p => [p.role?.id, p.draftRole?.id])
            .filter((id): id is string => id !== undefined)
    );

    return (
        <BasePanel title={`Set Bluffs for ${player.name}`} isOpen={isOpen} onClose={onClose} className="bluff-planner">
            <div ref={containerRef} className="token-panel">
                <div className="bluff-slots">
                    {slots.map((slot, index) => (
                        <div
                            key={index}
                            className={`bluff-slot${pendingRole ? ' slot-pending-target' : ''}`}
                            onClick={() => handleSlotClick(index)}
                        >
                            <Token role={slot} size={dynamicSize} customName={slot ? undefined : `Bluff ${index + 1}`}/>
                        </div>
                    ))}
                </div>
                {pendingRole && (
                    <p className="bluff-pending-hint">
                        Choose a slot for {pendingRole.name} (or click it again to cancel)
                    </p>
                )}

                <TeamGroup
                    name="Townsfolk"
                    roles={script?.townsfolk}
                    tokenSize={dynamicSize}
                    onClick={handleRoleClick}
                    selectedRoleIds={selectedRoleIds}
                    inPlayRoleIds={inPlayRoleIds}
                />

                <div className="outsider-minion-demon-group">
                    <TeamGroup
                        name="Outsiders"
                        roles={script?.outsiders}
                        tokenSize={dynamicSize}
                        onClick={handleRoleClick}
                        selectedRoleIds={selectedRoleIds}
                        inPlayRoleIds={inPlayRoleIds}
                    />
                    <TeamGroup
                        name="Minions"
                        roles={script?.minions}
                        tokenSize={dynamicSize}
                        onClick={handleRoleClick}
                        selectedRoleIds={selectedRoleIds}
                        inPlayRoleIds={inPlayRoleIds}
                    />
                    <TeamGroup
                        name="Demons"
                        roles={script?.demons}
                        tokenSize={dynamicSize}
                        onClick={handleRoleClick}
                        selectedRoleIds={selectedRoleIds}
                        inPlayRoleIds={inPlayRoleIds}
                    />
                </div>
            </div>
        </BasePanel>
    )
};
