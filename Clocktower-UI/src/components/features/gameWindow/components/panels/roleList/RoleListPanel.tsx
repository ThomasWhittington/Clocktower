import {BasePanel} from "@/components/ui";

interface RoleListPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const RoleListPanel = ({isOpen, onClose}: RoleListPanelProps) => {
    return (
        <BasePanel title="Role List" isOpen={isOpen} onClose={onClose} className="role-list-panel">
            <h2>dummy</h2>
        </BasePanel>
    )
};
