import {
    DiscordUserStatus,
    IconButton
} from "@/components/ui";
import {HelpIcon} from "@/components/ui/icons";
import {useDropdown} from "@/hooks";
import {useCurrentUserIsStoryteller} from "@/components/features/discordTownPanel/hooks";

interface RightHudProps {
    onRoleListClick: () => void;
    onNightOrderClick: () => void;
    onForceUpdateClick: () => void;
}

export const RightHud = ({
                             onRoleListClick,
                             onNightOrderClick,
                             onForceUpdateClick
                         }: RightHudProps) => {
    const {isOpen, toggle, close, dropdownRef} = useDropdown();
    const isStoryteller = useCurrentUserIsStoryteller();
    return (
        <div className="controls-right">
            <DiscordUserStatus/>
            <div className="relative" ref={dropdownRef}>
                <IconButton
                    icon={<HelpIcon/>}
                    isActive={isOpen}
                    onClick={toggle}
                    tooltip="Help Menu"
                />
                {isOpen && (
                    <div className="help-menu">
                        <h2>Help</h2>
                        <button type="button" onClick={() => {
                            onRoleListClick();
                            close();
                        }}>
                            <span>Role List</span>
                            <span>[R]</span>
                        </button>
                        <button type="button" onClick={() => {
                            onNightOrderClick();
                            close();
                        }}>
                            <span>Night Order</span>
                            <span>[N]</span>
                        </button>
                        {isStoryteller &&
                            <button type="button" className="text-discord-warning" onClick={() => {
                                onForceUpdateClick();
                                close();
                            }}>
                                Force Update
                            </button>
                        }
                    </div>
                )}
            </div>
        </div>
    );
}