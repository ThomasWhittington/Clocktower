import {Role} from "@/types";
import {Token} from "@/components/tokens";

interface TokenGroupProps {
    name: "Townsfolk" | "Outsiders" | "Minions" | "Demons" | "Travellers",
    roles?: Role[],
    tokenSize: number,
    onClick?: (role: Role) => void;
    selectedRoleIds?: Set<string>;
}

export const TeamGroup = ({name, roles, tokenSize, onClick, selectedRoleIds}: TokenGroupProps) => {
    if (roles && roles.length <= 0) return null;
    const className = `role-group ${name.toLowerCase()}`;
    return (
        <>
            {roles && roles.length > 0 && (
                <div className={className}>
                    <h3>{name}</h3>
                    <div>
                        {roles.map((role) => {
                            const isSelected = selectedRoleIds?.has(role.id) ?? false;
                            return (
                                <Token
                                    role={role}
                                    size={tokenSize}
                                    key={role.id}
                                    onClick={() => onClick?.(role)}
                                    className={isSelected ? 'selected-role' : 'unselected-role'}
                                />
                            );
                        })}
                    </div>
                </div>
            )}
        </>
    );
};