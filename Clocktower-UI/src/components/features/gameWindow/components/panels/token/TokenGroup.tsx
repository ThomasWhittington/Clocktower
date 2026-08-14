import {Role} from "@/types";
import {Token} from "@/components/tokens";

interface TokenGroupProps {
    name: "Townsfolk" | "Outsiders" | "Minions" | "Demons" | "Travellers",
    roles?: Role[],
    tokenSize: number,
    onClick?: (role: Role) => void;
    currentRoleId?: string;
}

export const TokenGroup = ({name, roles, tokenSize, onClick, currentRoleId}: TokenGroupProps) => {
    if (roles && roles.length <= 0) return null;
    const className = `role-group ${name.toLowerCase()}`;
    return (
        <>
            {roles && roles.length > 0 && (
                <div className={className}>
                    <h3>{name}</h3>
                    <div>
                        {roles.map((role) => (
                            <div key={role.id} className="token-wrapper">
                                <Token
                                    role={role}
                                    size={tokenSize}
                                    onClick={() => onClick?.(role)}
                                    className={role.id === currentRoleId ? 'current-role' : undefined}
                                />
                                <span className="role-ability">{role.fullDescription}</span>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </>
    );
};