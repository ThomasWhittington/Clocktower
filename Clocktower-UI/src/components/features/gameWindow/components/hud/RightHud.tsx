import {
    DiscordUserStatus,
    IconButton,
    ServerStatus,
} from "@/components/ui";
import {HelpMenu} from "@/components/features/gameWindow/components/hud/components";
import {
    HandIcon,
    VoteIcon
} from "@/components/ui/icons";
import {useUser} from "@/components/features/discordTownPanel/hooks";
import {useNominationState} from "@/components/features/gameWindow/hooks";
import {useAppStore} from "@/store";
import {
    User,
    UserType
} from "@/types";
import {useEffect} from "react";

interface RightHudProps {
    onRoleListClick: () => void;
    onNightOrderClick: () => void;
    onVoteHistoryClick: () => void;
    onPaperClick: () => void;
    onForceUpdateClick: () => void;
    onNominateClick: (player: User) => void;
    onVoteClick: (player: User) => void;
    onCancelNomination: () => void;
}

export const RightHud = ({
                             onRoleListClick,
                             onNightOrderClick,
                             onVoteHistoryClick,
                             onPaperClick,
                             onForceUpdateClick,
                             onNominateClick,
                             onVoteClick,
                             onCancelNomination
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

    const canVote = thisUser ?
        nominationsEnabled &&
        isActiveNomination &&
        thisUser.userType === UserType.Player &&
        !thisUser.voteLocked &&
        (!thisUser.isDead || (thisUser.isDead && thisUser.hasVoteToken))
        : false;
    useEffect(() => {
        if (!canNominate) {
            onCancelNomination();
        }
    }, [canNominate, onCancelNomination]);

    return (
        <div className="controls-right">
            <div className="top-row">
                <ServerStatus/>
                <div className="right-column">
                    {currentUser?.avatarUrl &&
                        <DiscordUserStatus/>
                    }
                    <HelpMenu
                        onRoleListClick={onRoleListClick}
                        onNightOrderClick={onNightOrderClick}
                        onVoteHistoryClick={onVoteHistoryClick}
                        onPaperClick={onPaperClick}
                        onForceUpdateClick={onForceUpdateClick}
                    />
                </div>
            </div>
            {canNominate && thisUser &&
                <IconButton icon={<VoteIcon/>} variant="danger" tooltip="Nominate" onClick={() => onNominateClick(thisUser)}/>
            }
            {canVote && thisUser &&
                <IconButton icon={<HandIcon gradientId="shared-hand"/>} variant={thisUser.handUp ? "danger" : "primary"} tooltip="Vote" onClick={() => onVoteClick(thisUser)}/>
            }
        </div>
    );
}