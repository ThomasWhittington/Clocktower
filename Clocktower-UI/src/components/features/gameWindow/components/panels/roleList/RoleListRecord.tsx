import {Role, User} from "@/types";

interface RoleListRecordProps {
    role?: Role;
    players: User[];
}

export const RoleListRecord = ({role, players}: RoleListRecordProps) => {
        const playerNames = players
            .filter(player => player.role?.id === role?.id)
            .map(p => p.name)
            .join(', ');

        return (
            <li className="role-record townsfolk">
                <span className="role-icon" style={{backgroundImage: `url(/tokenParts/roles/${role?.id}.png)`}}/>
                <div className="role">
                    <span className="role-player">{playerNames}</span>
                    <span className="role-name">{role?.name}</span>
                    <span className="role-ability">{role?.description}</span>
                </div>
            </li>
        )
    }
;
