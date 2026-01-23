import {
    BasePanel,
    IconButton,
    RoleDistributionWidget
} from "@/components/ui";
import {TeamGroup} from "./TeamGroup";
import {
    useElementSize,
    useServerHub
} from "@/hooks";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {RoleDistributionCounter} from "@/components/features/gameWindow/components";
import {useRolePlanner} from "@/components/features/gameWindow/hooks";
import {
    AssignIcon,
    RandomizeIcon,
    RemoveIcon
} from "@/components/ui/icons";
import {DiscordTownUtils} from "@/utils";
import {Role} from "@/types";
import type {
    Dispatch,
    SetStateAction
} from "react";

interface RolePlannerPanelProps {
    isOpen: boolean;
    onClose: () => void;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    selectedRoles: Role[];
    setSelectedRoles: Dispatch<SetStateAction<Role[]>>;
}

export const RolePlannerPanel = ({
                                     isOpen,
                                     onClose,
                                     setIsDraftMode,
                                     selectedRoles,
                                     setSelectedRoles
                                 }: RolePlannerPanelProps) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const dynamicSize = Math.min(parentSize.width, parentSize.height) / 8 || 64;
    const playerCount = DiscordTownUtils.getPlayerCountFromDistribution(discordTown);

    const {
        selectedRoleIds,
        hasSetupRoles,
        toggleRole,
        randomizeRoles,
        selectedSetupAffectingRoleNames,
        assignToDraft,
        clearRoles
    } = useRolePlanner({
        script,
        discordTown,
        setIsDraftMode,
        closePanel: onClose,
        selectedRoles,
        setSelectedRoles
    });

    return (
        <BasePanel title="Role Planner" isOpen={isOpen} onClose={onClose} className="role-planner">
            <div ref={containerRef} className="token-panel">
                {discordTown?.defaultRoleDistribution ?
                    <>
                        <p
                            className={`setup-warning ${hasSetupRoles ? 'opacity-100' : 'opacity-0 pointer-events-none'}`}
                            title={selectedSetupAffectingRoleNames}
                        >
                            WARNING - Setup is affected
                        </p>

                        <div className="flex w-full">
                            <IconButton icon={<RandomizeIcon/>} text="Randomize" variant="primary" onClick={randomizeRoles} className="flex-1"/>
                            <IconButton icon={<RemoveIcon/>} text="Clear" onClick={clearRoles} className="flex-1" isEnabled={selectedRoles.length > 0}/>
                            <IconButton icon={<AssignIcon/>} text="Assign to Draft [X]" variant="danger" onClick={assignToDraft} className="flex-1" isEnabled={playerCount > 0 && selectedRoles.length === playerCount}/>
                        </div>
                        <RoleDistributionCounter selectedRoles={selectedRoles} discordTown={discordTown}/>

                        <TeamGroup
                            name="Townsfolk"
                            roles={script?.townsfolk}
                            tokenSize={dynamicSize}
                            onClick={toggleRole}
                            selectedRoleIds={selectedRoleIds}
                        />

                        <div className="outsider-minion-demon-group">
                            <TeamGroup
                                name="Outsiders"
                                roles={script?.outsiders}
                                tokenSize={dynamicSize}
                                onClick={toggleRole}
                                selectedRoleIds={selectedRoleIds}
                            />
                            <TeamGroup
                                name="Minions"
                                roles={script?.minions}
                                tokenSize={dynamicSize}
                                onClick={toggleRole}
                                selectedRoleIds={selectedRoleIds}
                            />
                            <TeamGroup
                                name="Demons"
                                roles={script?.demons}
                                tokenSize={dynamicSize}
                                onClick={toggleRole}
                                selectedRoleIds={selectedRoleIds}
                            />
                        </div>
                    </>
                    : <RoleDistributionWidget/>
                }
            </div>
        </BasePanel>
    )
};