import {
    GroupIcon,
    HeartIcon,
    UserIcon,
    UsersIcon,
    VoteToken
} from "@/components/ui/icons";
import {useDiscordTown} from "@/components/features/discordTownPanel/hooks";
import {RoleType} from "@/types";

const getRoleIcon = (count: number | undefined) => {
    const Icon = count && count > 1 ? UsersIcon : UserIcon;
    return <Icon/>;
};
export const RoleDistributionWidget = () => {
    const {discordTown} = useDiscordTown();
    const players = discordTown?.players ?? [];
    const roleDistribution = discordTown?.defaultRoleDistribution;
    const playerCount = players.length ?? 0;
    const aliveCount = players.filter(o => !o.isDead).length ?? 0;
    const voteCount = players.filter(o => !o.isDead || o.hasVoteToken).length ?? 0;
    const travellerCount = players.filter(o => o.role?.type == RoleType.Traveller).length ?? 0;

    return (
        <div className="role-distribution">
            {playerCount > 0 &&
                <div>
                    <span className="players">{playerCount}{<GroupIcon/>}</span>
                    <span className="alive">{aliveCount}{<HeartIcon/>}</span>
                    <span className="votes">{voteCount}{<VoteToken/>}</span>
                </div>
            }

            {
                roleDistribution ?
                    <div>
                        <span className="townsfolk">{roleDistribution.townsfolk}{getRoleIcon(roleDistribution.townsfolk)}</span>
                        <span className="outsider">{roleDistribution.outsiders}{getRoleIcon(roleDistribution.outsiders)}</span>
                        <span className="minion">{roleDistribution.minions}{getRoleIcon(roleDistribution.minions)}</span>
                        <span className="demon">{roleDistribution.demons}{getRoleIcon(roleDistribution.demons)}</span>

                        {travellerCount > 0 &&
                            <span className="traveller">{travellerCount}{getRoleIcon(travellerCount)}</span>}
                    </div>
                    : <p>Add more players</p>
            }
        </div>
    );
}