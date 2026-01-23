import {
    useCallback,
    useMemo,
    useState
} from "react";
import type {RoleDistribution} from "@/types";
import {
    Role,
    Script
} from "@/types";
import {useAppStore} from "@/store";
import {useAction} from "@/hooks";

interface UseRolePlannerProps {
    script: Script | undefined;
    roleDistribution: RoleDistribution | undefined;
    setIsDraftMode: (callback: (prev: boolean) => boolean) => void;
    closePanel: () => void;
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

export const useRolePlanner = ({script, roleDistribution, setIsDraftMode, closePanel}: UseRolePlannerProps) => {
    const {runAction} = useAction();
    const {gameId} = useAppStore();
    const [selectedRoles, setSelectedRoles] = useState<Role[]>([]);

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
        if (!script || !roleDistribution) return;

        const randomRoles = getRolesByDistribution(script, roleDistribution);
        setSelectedRoles(randomRoles);
    }, [script, roleDistribution]);

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

    const assignToDraft = useCallback(async () => {
        if (!gameId) return;

        setIsDraftMode(() => true);

        await runAction(async () => {
            console.warn('All roles:', selectedRoles);
        });

        closePanel();
    }, [gameId, runAction, setIsDraftMode, closePanel, selectedRoles]);

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