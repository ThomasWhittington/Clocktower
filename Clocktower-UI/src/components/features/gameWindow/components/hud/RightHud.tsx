import {
    DiscordUserStatus,
    IconButton,
} from "@/components/ui";
import {HelpMenu} from "@/components/features/gameWindow/components/hud/components";
import {VoteIcon} from "@/components/ui/icons";
import {useUser} from "@/components/features/discordTownPanel/hooks";
import {useNominationState} from "@/components/features/gameWindow/hooks";
import {useAppStore} from "@/store";
import {
    User,
    UserType
} from "@/types";

interface RightHudProps {
    onRoleListClick: () => void;
    onNightOrderClick: () => void;
    onPaperClick: () => void;
    onForceUpdateClick: () => void;
    onNominateClick: (player: User) => void;
}

export const RightHud = ({
                             onRoleListClick,
                             onNightOrderClick,
                             onPaperClick,
                             onForceUpdateClick,
                             onNominateClick
                         }: RightHudProps) => {
    const {nominationsEnabled, isActiveNomination} = useNominationState();
    const {currentUser} = useAppStore();
    const {thisUser} = useUser(currentUser?.id);
    const canNominate = thisUser ?
        nominationsEnabled &&
        !isActiveNomination &&
        thisUser.userType === UserType.Player &&
        !thisUser.isDead
        : false;

    return (
        <div className="controls-right">
            <DiscordUserStatus/>
            <HelpMenu
                onRoleListClick={onRoleListClick}
                onNightOrderClick={onNightOrderClick}
                onPaperClick={onPaperClick}
                onForceUpdateClick={onForceUpdateClick}
            />
            {canNominate && thisUser &&
                <IconButton icon={<VoteIcon/>} variant="danger" tooltip="Nominate" onClick={() => onNominateClick(thisUser)}/>
            }
        </div>
    );
}