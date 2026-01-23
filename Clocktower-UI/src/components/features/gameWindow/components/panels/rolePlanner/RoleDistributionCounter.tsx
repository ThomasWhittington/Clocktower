import {
    DiscordTown,
    Role,
    RoleType
} from "@/types";

interface TokenGroupProps {
    discordTown: DiscordTown;
    selectedRoles: Role[];
}

export const RoleDistributionCounter = ({discordTown, selectedRoles}: TokenGroupProps) => {

    const getRoleCountStatus = (selectedCount: number, requiredCount: number) => {
        if (selectedCount > requiredCount) return 'too-many';
        if (selectedCount === requiredCount) return 'correct';
        return 'too-few';
    };

    const townsfolkCount = selectedRoles.filter(o => o.type === RoleType.Townsfolk).length;
    const outsidersCount = selectedRoles.filter(o => o.type === RoleType.Outsider).length;
    const minionsCount = selectedRoles.filter(o => o.type === RoleType.Minion).length;
    const demonsCount = selectedRoles.filter(o => o.type === RoleType.Demon).length;


    return (
        <div className="role-counts">
                            <span className={`role-count townsfolk ${getRoleCountStatus(townsfolkCount, discordTown?.defaultRoleDistribution?.townsfolk ?? 0)}`}>
                                <span>{townsfolkCount}</span>/{discordTown?.defaultRoleDistribution?.townsfolk}
                            </span>
            <span className={`role-count outsiders ${getRoleCountStatus(outsidersCount, discordTown?.defaultRoleDistribution?.outsiders ?? 0)}`}>
                                <span>{outsidersCount}</span>/{discordTown?.defaultRoleDistribution?.outsiders}
                            </span>
            <span className={`role-count minions ${getRoleCountStatus(minionsCount, discordTown?.defaultRoleDistribution?.minions ?? 0)}`}>
                                <span>{minionsCount}</span>/{discordTown?.defaultRoleDistribution?.minions}
                            </span>
            <span className={`role-count demons ${getRoleCountStatus(demonsCount, discordTown?.defaultRoleDistribution?.demons ?? 0)}`}>
                                <span>{demonsCount}</span>/{discordTown?.defaultRoleDistribution?.demons}
                            </span>
        </div>
    );
};