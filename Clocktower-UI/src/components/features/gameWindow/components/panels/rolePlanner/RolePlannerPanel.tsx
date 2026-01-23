import {
    BasePanel,
    RoleDistributionWidget
} from "@/components/ui";
import {TeamGroup} from "./TeamGroup";
import {
    useElementSize,
    useServerHub
} from "@/hooks";
import {Role} from "@/types";
import {
    useMemo,
    useState
} from "react";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {RoleDistributionCounter} from "@/components/features/gameWindow/components";

interface RolePlannerPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const RolePlannerPanel = ({isOpen, onClose}: RolePlannerPanelProps) => {
    const {script} = useServerHub();
    const {discordTown} = useDiscordTown();
    const {ref: containerRef, size: parentSize} = useElementSize<HTMLDivElement>();
    const dynamicSize = Math.min(parentSize.width, parentSize.height) / 8;
    const [selectedRoles, setSelectedRoles] = useState<Role[]>([]);

    const selectedRoleIds = useMemo(
        () => new Set(selectedRoles.map(r => r.id)),
        [selectedRoles]
    );
    const tokenClicked = async (role: Role | undefined) => {
        if (!role) return;

        const isAlreadySelected = selectedRoles.some(r => r.id === role.id);
        if (isAlreadySelected) {
            setSelectedRoles(selectedRoles.filter(r => r.id !== role.id));
        } else {
            setSelectedRoles([...selectedRoles, role]);
        }
    }

    return (
        <BasePanel title="Role Planner" isOpen={isOpen} onClose={onClose} className="role-planner">
            <div ref={containerRef} className="token-panel">
                {discordTown?.defaultRoleDistribution ?
                    <>
                        {selectedRoles.some(o => o.setup) && <p className="select-none">WARNING - Setup is affected</p>}

                        <RoleDistributionCounter selectedRoles={selectedRoles} discordTown={discordTown}/>

                        <TeamGroup
                            name="Townsfolk"
                            roles={script?.townsfolk}
                            tokenSize={dynamicSize}
                            onClick={tokenClicked}
                            selectedRoleIds={selectedRoleIds}
                        />

                        <div className="outsider-minion-demon-group">
                            <TeamGroup
                                name="Outsiders"
                                roles={script?.outsiders}
                                tokenSize={dynamicSize}
                                onClick={tokenClicked}
                                selectedRoleIds={selectedRoleIds}
                            />
                            <TeamGroup
                                name="Minions"
                                roles={script?.minions}
                                tokenSize={dynamicSize}
                                onClick={tokenClicked}
                                selectedRoleIds={selectedRoleIds}
                            />
                            <TeamGroup
                                name="Demons"
                                roles={script?.demons}
                                tokenSize={dynamicSize}
                                onClick={tokenClicked}
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
