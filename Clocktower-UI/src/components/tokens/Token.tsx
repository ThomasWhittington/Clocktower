import aliveTokenBase from '#/tokenParts/base/aliveToken.png';
import {LeftLeaf, OrangeLeaf, RightLeaf, RoleName, TokenRoleIcon, TopLeaf} from "./";
import {Role} from "@/types";
import {memo} from "react";

export const Token = memo(({role, size = 40, isDead}: { role?: Role, size?: number, isDead?: boolean }) => {
    const reminderLeaves = (role?.reminders || []).length + (role?.remindersGlobal || []).length;

    return (
        <div className={`token${isDead ? ' token-is-dead' : ''}`} style={{backgroundImage: `url(${aliveTokenBase})`, width: size, height: size}}>
            {role && <TokenRoleIcon role={role} className="token-icon"/>}
            {(Boolean(role?.firstNight) || role?.firstNightReminder) && <LeftLeaf/>}
            {(Boolean(role?.otherNight) || role?.otherNightReminder) && <RightLeaf/>}
            {reminderLeaves > 0 && <TopLeaf leafCount={reminderLeaves}/>}
            {role?.setup && <OrangeLeaf/>}
            {role?.name && <RoleName roleName={role.name}/>}
        </div>
    )
});