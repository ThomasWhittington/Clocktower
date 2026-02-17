import {
    Role,
    User
} from "@/types";
import {TokenRoleIcon} from "@/components/tokens";

interface RoleListRecordProps {
    role?: Role;
    players: User[];
}

export const RoleListRecord = ({role, players}: RoleListRecordProps) => {
    if (!role) return null;
    const playersWithRole = players.filter(player => player.role?.id === role?.id);

        return (
            <li className="role-record">
                <TokenRoleIcon roleId={role.id} className="role-icon"/>
                <div className="role">
                    <span className="role-player">
                        {playersWithRole.map((player, index) =>
                            <small className={`${player.isDead ? 'dead' : ''}`} key={player.id}>
                                {player.name + (playersWithRole.length > index + 1 ? ", " : "")}
                            </small>
                        )}
                    </span>
                    <span className="role-name">{role?.name}</span>
                    <span className="role-ability">{role?.description}</span>
                </div>
            </li>
        )
    }
;
