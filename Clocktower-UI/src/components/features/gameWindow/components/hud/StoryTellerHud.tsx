import {IconButton} from "@/components/ui";
import {AddUserIcon} from "@/components/ui/icons";

interface StoryTellerHudProps {
    inviteIsOpen: boolean;
    onInviteClick: () => void;
}

export const StoryTellerHud = ({inviteIsOpen, onInviteClick}: StoryTellerHudProps) => {
    return (
        <div className="controls-storyteller">
            <IconButton
                icon={<AddUserIcon/>}
                isActive={inviteIsOpen}
                onClick={onInviteClick}
            />
        </div>
    );
}