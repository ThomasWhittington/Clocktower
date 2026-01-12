import {User} from "@/types";
import aliveTokenBase from '#/tokenParts/base/aliveToken.png';
import {TokenRoleIcon} from "@/components/tokens/TokenRoleIcon.tsx";
import {OrangeLeaf} from "@/components/tokens/OrangeLeaf.tsx";
import {LeftLeaf} from "@/components/tokens/LeftLeaf.tsx";
import {RightLeaf} from "@/components/tokens/RightLeaf.tsx";
import {TopLeaf} from "@/components/tokens/TopLeaf.tsx";
import {RoleName} from "@/components/tokens/RoleName.tsx";

export const Token = ({player, size = 40}: { player: User, size?: number }) => {
    const role = player.role;
    const reminderLeaves = (role?.reminders || []).length + (role?.remindersGlobal || []).length;

    return (
        <div className={`token${player.isDead ? ' token-is-dead' : ''}`} style={{backgroundImage: `url(${aliveTokenBase})`, width: size, height: size}}>
            {role && <TokenRoleIcon role={role}/>}
            {(role?.firstNight || role?.firstNightReminder) && <LeftLeaf/>}
            {(role?.otherNight || role?.otherNightReminder) && <RightLeaf/>}
            {reminderLeaves > 0 && <TopLeaf leafCount={reminderLeaves}/>}
            {role?.setup && <OrangeLeaf/>}
            {role?.name && <RoleName roleName={role.name}/>}
        </div>
    )
};