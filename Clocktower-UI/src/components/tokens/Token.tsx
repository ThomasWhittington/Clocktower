import {
    LeftLeaf,
    OrangeLeaf,
    RightLeaf,
    RoleName,
    TokenRoleIcon,
    TopLeaf
} from "./";
import {Role} from "@/types";
import {memo} from "react";

interface TokenProps {
    role?: Role;
    size?: number;
    isDead?: boolean;
    onClick?: () => void;
    customName?: string;
    className?: string;
}


export const Token = memo(({role, size = 40, isDead, onClick, customName, className}: TokenProps) => {
    const aliveTokenBase = `${import.meta.env.BASE_URL}tokenParts/base/aliveToken.png`;
    const reminderLeaves = (role?.reminders || []).length + (role?.remindersGlobal || []).length;

    return (
        <span
            className={`token${isDead ? ' token-is-dead' : ''}${className ? ` ${className}` : ''}`}
            style={{backgroundImage: `url(${aliveTokenBase})`, width: size, height: size}}
            onClick={onClick}
        >
            {role && <TokenRoleIcon roleId={role.id} className="token-icon"/>}
            {(Boolean(role?.firstNight) || role?.firstNightReminder) && <LeftLeaf/>}
            {(Boolean(role?.otherNight) || role?.otherNightReminder) && <RightLeaf/>}
            {reminderLeaves > 0 && <TopLeaf leafCount={reminderLeaves}/>}
            {role?.setup && <OrangeLeaf/>}
            {role?.name ? <RoleName roleName={role.name}/> : <RoleName roleName={customName ?? ''}/>}
        </span>
    )
});