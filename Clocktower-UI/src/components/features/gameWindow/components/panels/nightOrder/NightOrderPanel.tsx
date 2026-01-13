import {BasePanel} from "@/components/ui";

interface NightOrderPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const NightOrderPanel = ({isOpen, onClose}: NightOrderPanelProps) => {
    return (
        <BasePanel title="Night Order" isOpen={isOpen} onClose={onClose} className="night-order-panel">
            <h2>dummy</h2>
        </BasePanel>
    )
};
