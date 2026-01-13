import {IconButton} from "@/components/ui";
import {AddUserIcon, ScriptIcon} from "@/components/ui/icons";

interface StoryTellerHudProps {
    inviteIsOpen: boolean;
    onInviteClick: () => void;
    scriptIsOpen: boolean;
    onScriptClick: () => void;
}

export const StoryTellerHud = (
    {
        inviteIsOpen,
        onInviteClick,
        scriptIsOpen,
        onScriptClick
    }: StoryTellerHudProps) => {
    return (
        <div className="controls-storyteller">
            <IconButton
                icon={<AddUserIcon/>}
                isActive={inviteIsOpen}
                onClick={onInviteClick}
            />
            <IconButton
                icon={<ScriptIcon/>}
                isActive={scriptIsOpen}
                onClick={onScriptClick}
            />
        </div>
    );
}