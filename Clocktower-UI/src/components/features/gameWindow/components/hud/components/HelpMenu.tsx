import {IconButton} from "@/components/ui";
import {useDropdown} from "@/hooks";
import {useCurrentUserIsStoryteller} from "@/components/features/discordTownPanel/hooks";
import {HelpIcon} from "@/components/ui/icons";

interface HelpMenuProps {
    onRoleListClick: () => void;
    onNightOrderClick: () => void;
    onVoteHistoryClick: () => void;
    onPaperClick: () => void;
    onForceUpdateClick: () => void;
}

export const HelpMenu = ({onRoleListClick, onNightOrderClick, onVoteHistoryClick, onPaperClick, onForceUpdateClick}: HelpMenuProps) => {
    const {isOpen, toggle, close, dropdownRef} = useDropdown();
    const isStoryteller = useCurrentUserIsStoryteller();
    return (
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
                    <button type="button" onClick={() => {
                        onVoteHistoryClick();
                        close();
                    }}>
                        <span>Vote History</span>
                        <span>[V]</span>
                    </button>
                    <button type="button" onClick={() => {
                        onPaperClick();
                        close();
                    }}>
                        <span>Paper</span>
                        <span>[P]</span>
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
    );
}
