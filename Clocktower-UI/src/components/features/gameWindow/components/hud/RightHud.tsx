import {DiscordUserStatus, IconButton} from "@/components/ui";
import {HelpIcon} from "@/components/ui/icons";
import {useDropdown} from "@/hooks";

interface StoryTellerHudProps {
    onRoleListClick: () => void;
    onNightOrderClick: () => void;
}

export const RightHud = ({
                             onRoleListClick,
                             onNightOrderClick
                         }: StoryTellerHudProps) => {
    const {isOpen, toggle, close, dropdownRef} = useDropdown();

    return (
        <div className="controls-right">
            <DiscordUserStatus/>
            <div className="relative" ref={dropdownRef}>
                <IconButton
                    icon={<HelpIcon/>}
                    isActive={isOpen}
                    onClick={toggle}
                />
                {isOpen && (
                    <div className="help-menu">
                        <h2>Help</h2>
                        <button onClick={() => {
                            onRoleListClick();
                            close();
                        }}>
                            <span>Role List</span>
                            <span>[R]</span>
                        </button>
                        <button onClick={() => {
                            onNightOrderClick();
                            close();
                        }}>
                            <span>Night Order</span>
                            <span>[N]</span>
                        </button>
                    </div>
                )}
            </div>
        </div>
    );
}