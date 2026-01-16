import {IconButton} from "@/components/ui";
import {AddUserIcon, DraftIcon, ScriptIcon} from "@/components/ui/icons";

interface StoryTellerHudProps {
    inviteIsOpen: boolean;
    onInviteClick: () => void;
    scriptIsOpen: boolean;
    onScriptClick: () => void;
    showDraftRoles: boolean;
    onDraftToggle: () => void;
}

export const StoryTellerHud = (
    {
        inviteIsOpen,
        onInviteClick,
        scriptIsOpen,
        onScriptClick,
        showDraftRoles,
        onDraftToggle
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
            <IconButton
                icon={<DraftIcon/>}
                isActive={showDraftRoles}
                isActiveVariant="danger"
                onClick={onDraftToggle}
            />
        </div>
    );
}