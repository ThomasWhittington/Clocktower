import {
    Role,
    RoleType,
    User
} from "@/types";
import {TokenRoleIcon} from "@/components/tokens";

interface NightOrderRecordProps {
    night: "first" | "other";
    role: Role;
    players: User[];
}

export const NightOrderRecord = ({night, role, players}: NightOrderRecordProps) => {
    if (!role) return null;

    const playersWithRole = role.id.startsWith("evil-")
        ? players.filter(player => player.role?.type === role.type)
        : players.filter(player => player.role?.id === role.id);

    const reminder = night === "first" ? role.firstNightReminder : role.otherNightReminder;
    
    return (
        <div className={`night-order-record ${night} ${RoleType[role.type].toLowerCase()}`}>
            {night === 'other' && <TokenRoleIcon role={role} className="role-icon"/>}
            <span className={`role-name ${playersWithRole.length === 0 ? 'no-players' : ''}`}>
                {role.name}
                <br/>
                <span className="role-player">
                    {playersWithRole.map((player, index) =>
                        <small className={`${player.isDead ? 'dead' : ''}`} key={player.id}>
                            {player.name + (playersWithRole.length > index + 1 ? ", " : "")}
                        </small>
                    )}
                </span>
            </span>
            {night === 'first' && <TokenRoleIcon role={role} className="role-icon"/>}
            {reminder && <span className="role-ability">{reminder}</span>}
        </div>
    )
};