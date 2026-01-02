import {BasePanel} from "@/components/ui";
import {AddUsers} from "./AddUsers";
import {UserManager} from "./UserManager";

interface UserManagerPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const UserManagerPanel = ({isOpen, onClose}: UserManagerPanelProps) => {
    return (
        <BasePanel title="User Manager" isOpen={isOpen} onClose={onClose} className="user-manager">
            <UserManager/>
            <AddUsers/>
        </BasePanel>
    )
};



