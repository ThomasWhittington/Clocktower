import {
    DiscordTown,
    Role,
    RoleType
} from "@/types";
import {DiscordTownUtils} from "@/utils";

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
    const roleCount = selectedRoles.length;
    const playerCount = DiscordTownUtils.getPlayerCountFromDistribution(discordTown);

    return (
        <div className="role-counts">
            <span className={`role-count players ${getRoleCountStatus(roleCount, playerCount)}`} title="Players">
                {roleCount}/{playerCount}
            </span>

            <span className={`role-count townsfolk ${getRoleCountStatus(townsfolkCount, discordTown?.defaultRoleDistribution?.townsfolk ?? 0)}`} title="Townsfolk">
                {townsfolkCount}/{discordTown?.defaultRoleDistribution?.townsfolk}
            </span>
            <span className={`role-count outsiders ${getRoleCountStatus(outsidersCount, discordTown?.defaultRoleDistribution?.outsiders ?? 0)}`} title="Outsiders">
                {outsidersCount}/{discordTown?.defaultRoleDistribution?.outsiders}
            </span>
            <span className={`role-count minions ${getRoleCountStatus(minionsCount, discordTown?.defaultRoleDistribution?.minions ?? 0)}`} title="Minions">
                {minionsCount}/{discordTown?.defaultRoleDistribution?.minions}
            </span>
            <span className={`role-count demons ${getRoleCountStatus(demonsCount, discordTown?.defaultRoleDistribution?.demons ?? 0)}`} title="Demons">
                {demonsCount}/{discordTown?.defaultRoleDistribution?.demons}
            </span>
        </div>
    );
};