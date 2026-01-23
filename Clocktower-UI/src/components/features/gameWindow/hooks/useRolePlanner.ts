import {
    type Dispatch,
    type SetStateAction,
    useCallback,
    useMemo
} from "react";
import {
    DiscordTown,
    Role,
    type RoleDistribution,
    Script
} from "@/types";
import {useAssignToDraft} from "@/components/features/gameWindow/hooks/";

interface UseRolePlannerProps {
    script: Script | undefined;
    discordTown: DiscordTown | undefined;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    closePanel: () => void;
    selectedRoles: Role[];
    setSelectedRoles: Dispatch<SetStateAction<Role[]>>;
}

const getRandomRoles = (roles: Role[] | undefined, count: number): Role[] => {
    if (!roles || roles.length === 0) return [];
    const shuffled = [...roles].sort(() => Math.random() - 0.5);
    return shuffled.slice(0, Math.min(count, shuffled.length));
};

const isRoleSelected = (roles: Role[], roleId: string): boolean => {
    return roles.some(r => r.id === roleId);
};

const getRolesByDistribution = (script: Script, distribution: RoleDistribution): Role[] => {
    return [
        ...getRandomRoles(script.townsfolk, distribution.townsfolk),
        ...getRandomRoles(script.outsiders, distribution.outsiders),
        ...getRandomRoles(script.minions, distribution.minions),
        ...getRandomRoles(script.demons, distribution.demons),
    ];
};

export const useRolePlanner = ({
                                   script,
                                   discordTown,
                                   setIsDraftMode,
                                   closePanel,
                                   selectedRoles,
                                   setSelectedRoles
                               }: UseRolePlannerProps) => {

    const {assignToDraft} = useAssignToDraft({
        selectedRoles,
        discordTown,
        setIsDraftMode,
        closePanel
    });

    const selectedRoleIds = useMemo(
        () => new Set(selectedRoles.map(r => r.id)),
        [selectedRoles]
    );

    const hasSetupRoles = useMemo(
        () => selectedRoles.some(o => o.setup),
        [selectedRoles]
    );

    const toggleRole = useCallback((role: Role | undefined) => {
        if (!role) return;

        setSelectedRoles(prev =>
            isRoleSelected(prev, role.id)
                ? prev.filter(r => r.id !== role.id)
                : [...prev, role]
        );
    }, []);

    const randomizeRoles = useCallback(() => {
        if (!script || !discordTown?.defaultRoleDistribution) return;

        const randomRoles = getRolesByDistribution(script, discordTown.defaultRoleDistribution);
        setSelectedRoles(randomRoles);
    }, [script, discordTown?.defaultRoleDistribution]);

    const clearRoles = useCallback(() => {
        setSelectedRoles([]);
    }, []);

    const selectedSetupAffectingRoleNames = useMemo(
        () => selectedRoles
            .filter(r => r.setup)
            .map(r => r.name || r.id)
            .join(', '),
        [selectedRoles]
    );

    return {
        selectedRoles,
        selectedRoleIds,
        hasSetupRoles,
        toggleRole,
        randomizeRoles,
        clearRoles,
        selectedSetupAffectingRoleNames,
        assignToDraft
    };
};