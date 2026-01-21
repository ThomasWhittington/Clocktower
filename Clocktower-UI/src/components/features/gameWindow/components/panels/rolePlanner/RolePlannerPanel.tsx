import {BasePanel} from "@/components/ui";

interface RolePlannerPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const RolePlannerPanel = ({isOpen, onClose}: RolePlannerPanelProps) => {
    return (
        <BasePanel title="Role Planner" isOpen={isOpen} onClose={onClose} className="role-planner">
            <p>WIP</p>
        </BasePanel>
    )
};
